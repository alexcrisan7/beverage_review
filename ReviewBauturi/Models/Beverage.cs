using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.UI.Xaml;

namespace ReviewBauturi.Models
{
    public class Beverage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RestaurantLocation { get; set; } = string.Empty;
        public string AddedBy { get; set; } = string.Empty; // Salvăm doar username-ul

        public double Price { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        // Proprietăți pentru afișare în interfață
        public string PriceDisplay => $"{Price} RON";
        public string RatingDisplay => $"Rating: {Rating}/5";
        public string LocationDisplay => string.IsNullOrEmpty(RestaurantLocation) ? "" : $"📍 {RestaurantLocation}";
        public string AddedByDisplay => string.IsNullOrEmpty(AddedBy) ? "" : $"Adăugat de: {AddedBy}";

        // Proprietăți pentru controlul butonului de ștergere (NU se salvează în baza de date)
        [NotMapped]
        public bool CanDelete { get; set; }

        [NotMapped]
        public Visibility DeleteButtonVisibility => CanDelete ? Visibility.Visible : Visibility.Collapsed;
    }
}