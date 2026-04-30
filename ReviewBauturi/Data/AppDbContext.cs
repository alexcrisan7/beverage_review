using Microsoft.EntityFrameworkCore;
using ReviewBauturi.Models;
using System;
using System.IO;

namespace ReviewBauturi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Beverage> Beverages { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // V4 este crucial aici ca să creeze tabelele pentru poze!
            string dbPath = Path.Combine(localFolder, "ReviewBauturiV4.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}