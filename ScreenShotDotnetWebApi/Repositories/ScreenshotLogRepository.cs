using ScreenshotTool.Models;
using Microsoft.EntityFrameworkCore;
namespace ScreenshotTool.Repositories
{
    public class ScreenshotLogRepository
    {
        private readonly AppDbContext _context;

        public ScreenshotLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveLogAsync(ScreenshotLog log)
        {
            try
            {
                _context.ScreenshotLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while saving data: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }

        }

        public async Task<ScreenshotLog?> GetPreviousLogAsync(ScreenshotLog currentLog)
        {
            var yesterday = DateTime.Now.Date.AddDays(-1);

            return await _context.ScreenshotLogs
                .Where(x =>
                    x.Url == currentLog.Url 
                    //&&
                    //x.LanguageId == currentLog.LanguageId &&
                    //x.AccountId == currentLog.AccountId &&
                    //x.ProjectSlug == currentLog.ProjectSlug 
                    //&&   x.CreatedAt.Date == yesterday
                    )
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }

}
