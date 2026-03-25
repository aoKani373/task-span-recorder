using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskSpanRecorder.Models;

namespace TaskSpanRecorder.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<TaskSpan> TaskSpans { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "taskspan.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskCategory>().HasData(
                new TaskCategory { Id = -1, Name = "☕空き時間" },
                new TaskCategory { Id = 1, Name = "💻開発" },
                new TaskCategory { Id = 2, Name = "📅会議" }
            );
        }
    }
}
