using Microsoft.Playwright;
using Polly.Registry;
using Polly;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Polly.Retry;

namespace ScreenshotTool
{
    public class ScreenShot
    {

        public async Task<string> CaptureScreenshotAsync(string url, int width = 1920, string customCss = "", string folderPath = "", int delayMilliseconds = 0)
        {

           // var retryPolicy = Policy
           //.Handle<PlaywrightException>() 
           //.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
           //(exception, timeSpan, retryCount, context) =>
           //{
           //    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
           //});
            var now = DateTime.Now;
            var screenshotFolder = Path.Combine(folderPath, "screenshot", now.Year.ToString(), now.Month.ToString("D2"), now.Day.ToString("D2"));
            Directory.CreateDirectory(screenshotFolder);


            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = width, Height = 1080 }
            });

            var page = await context.NewPageAsync();

            //await retryPolicy.ExecuteAsync(async () =>
            //{

            //});
            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 200000
            });
            string titleOfPage = "untitled";
            titleOfPage = await page.TitleAsync();

            var filename = $"{titleOfPage}_{now:HH_mm_ss}.png";
            var filePath = Path.Combine(screenshotFolder, filename);


            if (!string.IsNullOrWhiteSpace(customCss))
            {
                await page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = customCss });
            }

            if (delayMilliseconds > 0)
            {
                await page.WaitForTimeoutAsync(delayMilliseconds);
            }

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = filePath,
                FullPage = true
            });

            return filePath;
        }
        //public async Task<string> CaptureScreenshotAsync(string url, int width = 1920, string customCss = "", string folderPath = "", int delayMilliseconds = 0)
        //{
        //    var services = new ServiceCollection();
        //    services.AddResiliencePipeline("screenshot-pipeline", builder =>
        //    {
        //        builder
        //            .AddRetry(new RetryStrategyOptions
        //            {
        //                MaxRetryAttempts = 3,
        //                Delay = TimeSpan.FromSeconds(2)
        //            })
        //            .AddTimeout(TimeSpan.FromSeconds(210));
        //    });

        //    var serviceProvider = services.BuildServiceProvider();

        //    var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        //    var pipeline = pipelineProvider.GetPipeline("screenshot-pipeline");

        //    return await pipeline.ExecuteAsync(async token =>
        //    {
        //        var now = DateTime.Now;
        //        var screenshotFolder = Path.Combine(folderPath, "screenshot", now.Year.ToString(), now.Month.ToString("D2"), now.Day.ToString("D2"));
        //        Directory.CreateDirectory(screenshotFolder);

        //        using var playwright = await Playwright.CreateAsync();
        //        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //        {
        //            Headless = true
        //        });

        //        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        //        {
        //            ViewportSize = new ViewportSize { Width = width, Height = 1080 }
        //        });

        //        var page = await context.NewPageAsync();

        //        await page.GotoAsync(url, new PageGotoOptions
        //        {
        //            WaitUntil = WaitUntilState.NetworkIdle,
        //            Timeout = 200000
        //        });

        //        string titleOfPage = await page.TitleAsync();
        //        var filename = $"{titleOfPage}_{now:HH_mm_ss}.png";
        //        var filePath = Path.Combine(screenshotFolder, filename);

        //        if (!string.IsNullOrWhiteSpace(customCss))
        //        {
        //            await page.AddStyleTagAsync(new PageAddStyleTagOptions { Content = customCss });
        //        }

        //        if (delayMilliseconds > 0)
        //        {
        //            await page.WaitForTimeoutAsync(delayMilliseconds);
        //        }

        //        await page.ScreenshotAsync(new PageScreenshotOptions
        //        {
        //            Path = filePath,
        //            FullPage = true
        //        });

        //        return filePath;

        //    }, CancellationToken.None); 
        //}


    }
}
