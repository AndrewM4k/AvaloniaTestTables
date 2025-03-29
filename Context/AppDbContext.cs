using AvaloniaTestTables.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace AvaloniaTestTables.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Mode> Modes { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TablesTestAvaloniaApp", "database.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();
        }
    }
}
