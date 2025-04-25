using ScreenshotTool.Models;
using ScreenshotTool.Repositories;
using ScreenshotTool.RequestLoggingMiddleware;
using ScreenshotTool.Services;
using Microsoft.EntityFrameworkCore;
using Polly.Retry;
using Polly;
using System.Threading;
using Polly.Registry;
using Microsoft.Playwright;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5998);
});
var app = builder.Build();
int exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
if (exitCode != 0)
{
    throw new Exception($"Failed to install Playwright with exit code {exitCode}.");
}
else
{
    Console.WriteLine("Playwright installation successful.");
}

var playwright = await Playwright.CreateAsync();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.UseRouting();

app.MapControllers();
app.Run();