using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReviewBauturi.Models
{
    public class Beverage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RestaurantLocation { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string AddedBy { get; set; } = string.Empty;

        // --- NOU: CÂMPUL PENTRU POZE ---
        public string? ImagePath { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; }

        [NotMapped]
        public string PriceDisplay => Price > 0 ? $"{Price} Lei" : "Preț nespecificat";

        [NotMapped]
        public string LocationDisplay => string.IsNullOrEmpty(RestaurantLocation) ? "Locație necunoscută" : RestaurantLocation;

        [NotMapped]
        public string AddedByDisplay => $"Adăugat de {AddedBy}";

        [NotMapped]
        public Visibility DeleteButtonVisibility => CanDelete ? Visibility.Visible : Visibility.Collapsed;

        // Converteste calea pozei in imagine pt interfață
        [NotMapped]
        public BitmapImage? ImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath)) return null;
                return new BitmapImage(new Uri(ImagePath));
            }
        }

        [NotMapped]
        public Visibility HasImage => string.IsNullOrEmpty(ImagePath) ? Visibility.Collapsed : Visibility.Visible;
    }
}