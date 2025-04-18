using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace ScreenshotTool.Controllers
{
    [ApiController]
    [Route("api")]
    public class ScreenShotController : Controller
    { 
        private readonly ScreenShot _screenShot;
        public ScreenShotController()
        {
            _screenShot = new ScreenShot();
        }


        [HttpGet("screen-shot")]
        public async Task<IActionResult> GetScreenshot2([FromQuery] string url, [FromQuery] int? width, [FromQuery] string? customCss, [FromQuery] string? folderPath, [FromQuery] int? delay)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL trống");


            int resolvedWidth = width ?? 1920;
            if (width <= 300)
            {
                return BadRequest("Width không được nhỏ hơn hoặc bằng 300");
            }
            string resolvedCss;
            if (string.IsNullOrEmpty(customCss))
            {
                resolvedCss = ".hide.hidden {\r\ndisplay: block !important;\r\nbackground-color: rgba(251, 233, 233, 255);\r\n}";
            }
            else if (customCss.Trim() == "0")
            {
                resolvedCss = "";
            }
            else
            {
                resolvedCss = customCss;
            }
            var path = await _screenShot.CaptureScreenshotAsync(url, resolvedWidth, resolvedCss, folderPath, delay ?? 0);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(path);

            return File(fileBytes, "image/png", Path.GetFileName(path));
        }

    }
}
