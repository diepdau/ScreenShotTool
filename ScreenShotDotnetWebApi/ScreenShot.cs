using Microsoft.Playwright;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace ScreenshotTool
{
    public class ScreenShot
    {

        public async Task<string> CaptureScreenshotAsync(string url, int width = 1920, string customCss = "", string folderPath = "", int delayMilliseconds = 0)
        {
            string titleOfPage = "untitled";
            try
            {
                var uri = new Uri(url);
                var segments = uri.Segments.Select(s => s.Trim('/')).ToList();
                var pageIndex = segments.FindIndex(s => s.Equals("page", StringComparison.OrdinalIgnoreCase));

                if (pageIndex >= 0 && pageIndex + 1 < segments.Count)
                {
                    titleOfPage = segments[pageIndex + 1];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Error parsing title]: {e.Message}");
            }

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                folderPath = Path.Combine(Path.GetTempPath(), "screenshots");
            }

            var now = DateTime.Now;
            var screenshotFolder = Path.Combine(folderPath, "screenshot", now.Year.ToString(), now.Month.ToString("D2"), now.Day.ToString("D2"));
            Directory.CreateDirectory(screenshotFolder);

            var filename = $"{titleOfPage}_{now:HH_mm_ss}.png";
            var filePath = Path.Combine(screenshotFolder, filename);

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

            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 200000
            });

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
