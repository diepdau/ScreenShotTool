namespace ScreenshotTool.Models
{
    public class ScreenshotRequest
    {
            public string Url { get; set; }
            public int? Width { get; set; }
            public string? CustomCss { get; set; }
            public string? FolderPath { get; set; }
            public int? Delay { get; set; }
            public string? LanguageId { get; set; }
            public string? AccountId { get; set; }
            public string? ProjectSlugs { get; set; }
        

    }
}
