using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScreenshotTool
{
    public static class InstallPlaywrightIfNeeded
    {
        public static async Task InstallPlaywright()
        {
            var browsersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-playwright");

            //if (!Directory.Exists(browsersPath) || Directory.GetDirectories(browsersPath).Length == 0)
            //{
            //    Console.WriteLine("📦 Playwright browsers not found. Installing...");
            //    Microsoft.Playwright.Program.Main(new[] { "install" });
            //    Console.WriteLine("✅ Playwright browsers installed.");
            //}
            //else
            //{
            //    Console.WriteLine("✅ Playwright already installed.");
            //}
        }
    }
}
