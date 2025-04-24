using ScreenshotTool.Models;
using ScreenshotTool.Repositories;

namespace ScreenshotTool.Services
{
    public class ScreenshotComparisonService
    {
        private readonly ScreenshotLogRepository _repository;

        public ScreenshotComparisonService(ScreenshotLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsTodayScreenshotSimilarToYesterdayAsync(ScreenshotLog todayLog)
        {
            var yesterdayLog = await _repository.GetPreviousLogAsync(todayLog);
            if (yesterdayLog == null || !File.Exists(yesterdayLog.ImagePath) || !File.Exists(todayLog.ImagePath))
                return false;

            return ScreenshotComparer.AreImagesSimilar(yesterdayLog.ImagePath, todayLog.ImagePath);
        }
    }

}
