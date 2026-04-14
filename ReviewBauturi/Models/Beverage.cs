using System.Collections.Generic;

namespace ReviewBauturi.Models
{
    public class Beverage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // Stocăm prețul și rating-ul mediu direct pe obiectul băutură pentru simplitate în Release 1
        public double Price { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        // Proprietăți ajutătoare pentru afișarea corectă în interfață
        public string PriceDisplay => $"{Price} RON";
        public string RatingDisplay => $"Rating: {Rating}/5";
    }
}