using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using ReviewBauturi.Models;

namespace ReviewBauturi.Data
{
    public partial class AppDbContext : DbContext
    {
        // Tabelele din baza de date
        public DbSet<Beverage> Beverages { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Creăm fișierul bazei de date în folderul local al aplicației
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "beverages.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}