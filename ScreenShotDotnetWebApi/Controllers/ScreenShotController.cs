using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ScreenshotTool.Models;
using System.Security.Policy;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ScreenshotTool.Interface;
namespace ScreenshotTool.Controllers
{
    [ApiController]
    [Route("api")]
    public class ScreenShotController : Controller
    {
        private readonly ScreenShot _screenShot;
        private readonly ScreenshotDbContext _context;
        private readonly IScreenshotService _screenshotService;
        public ScreenShotController(ScreenshotDbContext context, IScreenshotService screenshotService)
        {
            _screenShot = new ScreenShot();
            _context = context;
            _screenshotService = screenshotService;

        }



        //[HttpPost("screen-shot")]
        //public async Task<IActionResult> GetScreenshot2([FromBody] ScreenShotRequest1 request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    int resolvedWidth = request.Width ?? 1920;
        //    if (resolvedWidth <= 300)
        //    {
        //        return BadRequest("Width không được nhỏ hơn hoặc bằng 300");
        //    }

        //    string resolvedCss = request.CustomCss switch
        //    {
        //        null => ".hide.hidden {\r\ndisplay: block !important;\r\nbackground-color: rgba(251, 233, 233, 255);\r\n}",
        //        "0" => "",
        //        _ => request.CustomCss
        //    };

        //    var newImagePath = await _screenShot.CaptureScreenshotAsync(
        //        request.Url,
        //        resolvedWidth,
        //        resolvedCss,
        //        request.FolderPath,
        //        request.Delay ?? 0
        //    );

        //    var previousImageLog = await _context.ScreenshotLogs
        //        .Where(x => x.Url == request.Url)
        //        .OrderByDescending(x => x.CreatedAt)
        //        .FirstOrDefaultAsync();

        //    bool isSame = false;
        //    string? diffPath = null;

        //    if (previousImageLog != null && System.IO.File.Exists(previousImageLog.ImagePath))
        //    {
        //        try
        //        {
        //            Mat? diffMat;
        //            isSame = CompareImagesWithEmgu(previousImageLog.ImagePath, newImagePath, out diffMat);

        //            if (!isSame && diffMat != null)
        //            {
        //                string diffFolder = Path.Combine(request.FolderPath ?? Path.GetTempPath(), "diff");
        //                Directory.CreateDirectory(diffFolder);

        //                string diffFileName = $"diff_{DateTime.Now:HHmmss}.png";
        //                diffPath = Path.Combine(diffFolder, diffFileName);

        //                diffMat.Save(diffPath);
        //                Console.WriteLine($"Diff image saved to: {diffPath}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Lỗi so sánh ảnh: {ex.Message}");
        //        }
        //    }

        //    if (!isSame)
        //    {
        //        int? height = null;
        //        using (var img = System.Drawing.Image.FromFile(newImagePath))
        //        {
        //            height = img.Height;
        //        }

        //        var log = new ScreenshotLog
        //        {
        //            Url = request.Url,
        //            ImagePath = newImagePath,
        //            Width = resolvedWidth,
        //            Height = height,
        //            CreatedAt = DateTime.UtcNow,
        //            LanguageId = request.LanguageId,
        //            AccountId = request.AccountId,
        //            ProjectSlug = request.ProjectSlug
        //        };

        //        _context.ScreenshotLogs.Add(log);
        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok(new
        //    {
        //        message = isSame
        //            ? "Ảnh mới giống ảnh trước đó. Không cần lưu."
        //            : "Screenshot đã được lưu thành công.",
        //        imagePath = newImagePath,
        //        isSameAsPrevious = isSame
        //    });
        //}

        //[HttpGet("screenshot-log")]
        //public async Task<IActionResult> GetLogByUrl([FromQuery] string url)
        //{
        //    var logs = await _context.ScreenshotLogs
        //        .Where(x => x.Url == url)
        //        .OrderByDescending(x => x.CreatedAt)
        //        .ToListAsync();

        //    return Ok(logs);
        //}

        //private bool CompareImagesWithEmgu(string imgPath1, string imgPath2, out Mat? diffOutput)
        //{
        //    using var img1 = CvInvoke.Imread(imgPath1, ImreadModes.Color);
        //    using var img2 = CvInvoke.Imread(imgPath2, ImreadModes.Color);

        //    diffOutput = null;

        //    if (img1.Size != img2.Size)
        //        return false;

        //    // So sánh sự khác biệt tuyệt đối
        //    var diff = new Mat();
        //    CvInvoke.AbsDiff(img1, img2, diff);

        //    // Chuyển sang grayscale để dễ lọc
        //    var gray = new Mat();
        //    CvInvoke.CvtColor(diff, gray, ColorConversion.Bgr2Gray);

        //    // Nổi bật điểm khác biệt (threshold > 30)
        //    var thresh = new Mat();
        //    CvInvoke.Threshold(gray, thresh, 30, 255, ThresholdType.Binary);

        //    // Đếm số pixel khác biệt
        //    int nonZero = CvInvoke.CountNonZero(thresh);

        //    if (nonZero > 100) 
        //    {
        //        diffOutput = diff;
        //        return false;
        //    }

        //    return true;
        //}


        //tests

        //[HttpPost("screen-shot123")]
        //public async Task<IActionResult> GetScreenshot([FromBody] ScreenShotRequest1 request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        var (imagePath, isSame, diffPath) = await _screenshotService.ProcessScreenshotAsync(request);

        //        return Ok(new
        //        {
        //            message = isSame
        //                ? "Ảnh mới giống ảnh trước đó. Không cần lưu."
        //                : "Screenshot đã được lưu thành công.",
        //            imagePath,
        //            isSameAsPrevious = isSame,
        //            diffPath
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Lỗi xử lý ảnh: {ex.Message}" });
        //    }
        //}
        [HttpPost("screen-shot123")]
        public async Task<IActionResult> GetScreenshot([FromBody] ScreenShotRequest1 request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var (imagePath, isSame, diffPath) = await _screenshotService.ProcessScreenshotAsync(request);

                // Đọc file thành mảng byte
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(imagePath);

                // Convert sang base64
                string base64Image = Convert.ToBase64String(fileBytes);

                return Ok(new
                {
                    message = isSame
                        ? "Ảnh mới giống ảnh trước đó. Không cần lưu."
                        : "Screenshot đã được lưu thành công.",
                    imagePath,
                    isSameAsPrevious = isSame,
                    diffPath,
                    base64Image = $"data:image/png;base64,{base64Image}" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi xử lý ảnh: {ex.Message}" });
            }
        }

        [HttpGet("screenshot-log")]
        public IActionResult GetLogByUrl([FromQuery] string url)
        {
            var logs = _context.ScreenshotLogs
                .Where(x => x.Url == url)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(logs);
        }
    }

}

