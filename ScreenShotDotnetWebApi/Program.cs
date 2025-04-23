using ScreenshotTool.Models;
using ScreenshotTool.Repositories;
using ScreenshotTool.RequestLoggingMiddleware;
using ScreenshotTool.Services;
using Microsoft.EntityFrameworkCore;
using Polly.Retry;
using Polly;
using System.Threading;
using Polly.Registry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ScreenshotLogRepository>();
builder.Services.AddScoped<ScreenshotComparisonService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var services = new ServiceCollection();

services.AddResiliencePipeline("screenshot-pipeline", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2)
        })
        .AddTimeout(TimeSpan.FromSeconds(15));
});

var serviceProvider = services.BuildServiceProvider();
var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

// Lấy pipeline ra để sử dụng
var screenshotPipeline = pipelineProvider.GetPipeline("screenshot-pipeline");


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
