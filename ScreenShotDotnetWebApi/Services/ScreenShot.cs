using Microsoft.Playwright;
using Polly.Registry;
using Polly;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Polly.Retry;
using Polly.Timeout;
namespace ScreenshotTool.Services
{
    public class ScreenShot
    {
    
        public async Task<string> CaptureScreenshotAsync(string url, int width = 1920, string customCss = "", string folderPath = "", int delayMilliseconds = 0)
        {
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
            var retryGotoPolicy = Policy
             .Handle<TimeoutException>()
             .Or<PlaywrightException>()
             .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
                 (exception, timeSpan, retryCount, context) =>
                 {
                     Console.WriteLine($"[GotoAsync] Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                 });

            await retryGotoPolicy.ExecuteAsync(async () =>
            {
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 200000
                });

            });

            await page.WaitForFunctionAsync("() => document.title && document.title !== 'about:blank'");
            string titleOfPage = await page.TitleAsync();

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
    }
}
