using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using ReviewBauturi.Models;

namespace ReviewBauturi.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<Beverage> Beverages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 1. Luăm calea către folderul "Documente" al PC-ului tău
            string folderDocumente = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // 2. Creăm un folder dedicat pentru aplicația noastră ca să fie curat
            string folderAplicatie = Path.Combine(folderDocumente, "ReviewBauturi");

            // Ne asigurăm că folderul există (dacă nu, îl creăm)
            if (!Directory.Exists(folderAplicatie))
            {
                Directory.CreateDirectory(folderAplicatie);
            }

            // 3. Setăm calea finală a fișierului .db
            string dbPath = Path.Combine(folderAplicatie, "beverages.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123", IsAdmin = true }
            );
        }
    }
}