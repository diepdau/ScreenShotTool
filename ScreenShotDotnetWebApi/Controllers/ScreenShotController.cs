using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ScreenshotTool.Controllers
{
    [ApiController]
    [Route("api/screen-shot")]
    public class ScreenShotController : Controller
    { 
        private readonly ScreenShot _screenShot;
        public ScreenShotController()
        {
            _screenShot = new ScreenShot();
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string url, [FromQuery] int? width, [FromQuery] string? customCss, [FromQuery] string? folderPath, [FromQuery] int? delay)
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
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return BadRequest(new { message = "chưa chọn folderPath" });
            }
            var filePath = await _screenShot.CaptureScreenshotAsync(url, resolvedWidth, resolvedCss, folderPath, delay ?? 0);
            return Ok(new { message = "Chụp màn hình thành công", path=filePath.FilePath, ms=filePath.ElapsedTime});
        }
    }
}
