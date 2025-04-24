using System.Collections.Generic;
using Emgu.CV.Features2D;
using Microsoft.EntityFrameworkCore;

namespace ScreenshotTool.Models
{
    //public class AppDbContext : DbContext
    //{
    //    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //    public DbSet<ScreenshotLog> ScreenshotLogs { get; set; }
    //}
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<ScreenshotLog> ScreenshotLogs { get; set; }

    }


}
