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


        [HttpPost("screen-shot")]
        public async Task<IActionResult> PostScreenshotCheck([FromBody] ScreenshotRequest request)
        {
            try
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
                if (string.IsNullOrWhiteSpace(request.FolderPath))
                {
                    return BadRequest(new { message = "chưa chọn folderPath" });
                }
                int delay = request.Delay.GetValueOrDefault(0);

                var screenshotPath = await _screenShot.CaptureScreenshotAsync(request.Url, width, customCss, request.FolderPath, delay);

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

                    return Ok(new { message = "Ảnh này giống ảnh hôm qua", path = screenshotPath });
                }

                await _logRepository.SaveLogAsync(tempLog);
                return Ok(new { message = "Chụp ảnh thành công", path = screenshotPath });
            }
            catch(Exception e)
            {
               return StatusCode(500, new { message = "Error: " + e.Message }); ;
            }
           
        }

        [HttpGet("select")]
        public IActionResult SelectFolder()
        {
            var selectedPath = "";

            var t = new Thread(() =>
            {
                using var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK) selectedPath = dialog.SelectedPath;
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (string.IsNullOrEmpty(selectedPath)) return BadRequest("No folder selected.");

            return Ok(new { folderPath = selectedPath });
        }

    }
}