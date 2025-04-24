namespace ScreenshotTool.Models
{
    public class ScreenshotLog
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Url { get; set; }
        public string ImagePath { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public string LanguageId { get; set; }
        public string AccountId { get; set; }
        public string ProjectSlug { get; set; }
    }
}
