using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System;
using ScreenshotTool.Models;
using ScreenshotTool.Repositories;
using ScreenshotTool.Services;
namespace ScreenshotTool.Controllers
{
    [ApiController]
    [Route("api")]
    public class ScreenShotController : Controller
    {
        private readonly ScreenShot _screenShot;
        private readonly ScreenshotLogRepository _logRepository;
        private readonly ScreenshotComparisonService _comparisonService;
        public ScreenShotController(ScreenshotLogRepository logRepository, ScreenshotComparisonService comparisonService)
        {
            _logRepository = logRepository;
            _comparisonService = comparisonService;
            _screenShot = new ScreenShot();
        }

        //[HttpPost("screen-shot")]
        //public async Task<IActionResult> PostScreenshot([FromBody] ScreenshotRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Url))
        //        return BadRequest("URL không được để trống");

        //    int width = request.Width.GetValueOrDefault(1920);
        //    if (width <= 300)
        //        return BadRequest("Chiều rộng không được nhỏ hơn hoặc bằng 300");

        //    string customCss = request.CustomCss switch
        //    {
        //        null => ".hide.hidden { display: block !important; background-color: rgba(251, 233, 233, 255); }",
        //        "0" => "",
        //        _ => request.CustomCss
        //    };

        //    string folderPath = request.FolderPath ?? string.Empty;
        //    int delay = request.Delay.GetValueOrDefault(0);
        //    var stopwatch = Stopwatch.StartNew();
        //    var path = await _screenShot.CaptureScreenshotAsync(request.Url, width, customCss, folderPath, delay);
        //    stopwatch.Stop();
        //    Console.WriteLine($"Thời gian chụp screenshot: {stopwatch.Elapsed.TotalSeconds} giây");
        //    var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
        //    return File(fileBytes, "image/png", Path.GetFileName(path));
        //}


        [HttpPost("screen-shot")]
        public async Task<IActionResult> PostScreenshotCheck([FromBody] ScreenshotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("URL không được để trống");

            int width = request.Width.GetValueOrDefault(1920);
            if (width <= 300)
                return BadRequest("Chiều rộng không được nhỏ hơn hoặc bằng 300");

            string customCss = request.CustomCss switch
            {
                null => ".hide.hidden { display: block !important; background-color: rgba(251, 233, 233, 255); }",
                "0" => "",
                _ => request.CustomCss
            };

            string folderPath = request.FolderPath ?? string.Empty;
            int delay = request.Delay.GetValueOrDefault(0);

            var screenshotPath = await _screenShot.CaptureScreenshotAsync(request.Url, width, customCss, folderPath, delay);

            // so sánh
            var tempLog = new ScreenshotLog
            {
                CreatedAt = DateTime.Now,
                Url = request.Url,
                Width = width,
                LanguageId = request.LanguageId,
                AccountId = request.AccountId,
                ProjectSlug = request.ProjectSlugs,
                ImagePath = screenshotPath
            };

            bool isSimilar = await _comparisonService.IsTodayScreenshotSimilarToYesterdayAsync(tempLog);

            if (isSimilar)
            {
                if (System.IO.File.Exists(screenshotPath))
                    System.IO.File.Delete(screenshotPath);

                //return Ok(new { message = "Ảnh này giống ảnh hôm qua", path = screenshotPath });
                return Ok(new { message = "Ảnh này giống ảnh hôm qua"});
            }

            // Khac,lưu log mới
            await _logRepository.SaveLogAsync(tempLog);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(screenshotPath);
            return File(fileBytes, "image/png", Path.GetFileName(screenshotPath));
        }


    }
}