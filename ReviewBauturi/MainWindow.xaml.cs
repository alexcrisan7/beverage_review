using System;
using System.Linq;
using Microsoft.UI.Xaml;
using ReviewBauturi.Data;
using ReviewBauturi.Models;

namespace ReviewBauturi
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            try
            {
                using var db = new AppDbContext();

                // ADAUGĂ ACEASTĂ LINIE TEMPORAR:
                db.Database.EnsureDeleted();

                // Acum va crea o bază de date complet nouă, cu noile coloane
                db.Database.EnsureCreated();

                IncarcaDatele();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EROARE FATALĂ BAZĂ DE DATE: {ex.Message}");
            }
        }

        private void IncarcaDatele()
        {
            using var db = new AppDbContext();

            // Sortăm să vedem cele mai noi review-uri sus
            ListaBauturi.ItemsSource = db.Beverages.OrderByDescending(b => b.Id).ToList();
        }

        private void AdaugaReview_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputNumeBautura.Text))
            {
                using (var db = new AppDbContext())
                {
                    var reviewNou = new Beverage
                    {
                        Name = InputNumeBautura.Text,
                        Category = InputCategorie.Text,
                        Price = InputPret.Value,
                        Rating = (int)InputRating.Value,
                        Comment = InputComentariu.Text
                    };

                    db.Beverages.Add(reviewNou);
                    db.SaveChanges();
                }

                // Resetăm câmpurile
                InputNumeBautura.Text = string.Empty;
                InputCategorie.Text = string.Empty;
                InputPret.Value = 0;
                InputRating.Value = 1;
                InputComentariu.Text = string.Empty;

                IncarcaDatele();
            }
        }
    }
}