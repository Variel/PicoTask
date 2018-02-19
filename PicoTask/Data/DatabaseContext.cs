using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PicoTask.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskCategory> Categories { get; set; }
        public DbSet<TaskItemCategory> TaskItemCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItemCategory>()
                .HasKey(j => new {j.TaskItemId, j.TaskCategoryId});

            modelBuilder.Entity<TaskItemCategory>()
                .HasOne(j => j.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(j => j.TaskCategoryId);

            modelBuilder.Entity<TaskItemCategory>()
                .HasOne(j => j.Task)
                .WithMany(t => t.Categories)
                .HasForeignKey(j => j.TaskItemId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
