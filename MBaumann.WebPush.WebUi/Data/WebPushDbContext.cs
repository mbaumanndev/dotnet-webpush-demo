using MBaumann.WebPush.WebUi.Entities;
using Microsoft.EntityFrameworkCore;

namespace MBaumann.WebPush.WebUi.Data
{
    public sealed class WebPushDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlite("Data Source=database.db");
        }

        public DbSet<Device> Devices { get; set; }

    }
}
