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
        public DbSet<TaskGroup> TaskGroups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "taskspan.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskGroup>().HasData(
                new TaskGroup { Id = -1, Name = "未割当て", ColorHex = "#FF808080" },
                new TaskGroup { Id = 1, Name = "グループA", ColorHex = "#FF0078D7" },
                new TaskGroup { Id = 2, Name = "グループB", ColorHex = "#FFD2691E" }
            );

            modelBuilder.Entity<TaskCategory>().HasData(
                new TaskCategory { Id = -1, Name = "☕空き時間", TaskGroupId = -1 },
                new TaskCategory { Id = 1, Name = "💻開発", TaskGroupId = 1 },
                new TaskCategory { Id = 2, Name = "📅会議", TaskGroupId = 2 }
            );
        }
    }
}
