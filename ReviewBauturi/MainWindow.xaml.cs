using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ReviewBauturi.Data;
using ReviewBauturi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace ReviewBauturi
{
    public sealed partial class MainWindow : Window
    {
        private User? _currentUser;
        private static readonly HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            this.InitializeComponent();

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "ReviewBauturiApp_StudentProject");
            }

            try
            {
                using var db = new AppDbContext();
                db.Database.EnsureCreated();
                // Nu apelăm IncarcaDatele aici, îl va apela Login-ul.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EROARE FATALĂ BAZĂ DE DATE: {ex.Message}");
            }
        }

        private void IncarcaDatele()
        {
            using var db = new AppDbContext();

            // Extragem toate băuturile din baza de date
            var bauturi = db.Beverages.OrderByDescending(b => b.Id).ToList();

            // 1. FILTRARE CATEGORIE
            if (FilterCategory.SelectedItem != null)
            {
                string categorie = FilterCategory.SelectedItem.ToString() ?? "";
                if (categorie != "Toate")
                {
                    bauturi = bauturi.Where(b => b.Category == categorie).ToList();
                }
            }

            // 2. FILTRARE LOCAȚIE (Căutare parțială / Case-insensitive)
            if (!string.IsNullOrWhiteSpace(FilterLocation.Text))
            {
                string locatieCautata = FilterLocation.Text.ToLower();
                bauturi = bauturi.Where(b => !string.IsNullOrEmpty(b.RestaurantLocation) &&
                                             b.RestaurantLocation.ToLower().Contains(locatieCautata)).ToList();
            }

            // 3. FILTRARE PREȚ MAXIM
            if (!double.IsNaN(FilterPrice.Value) && FilterPrice.Value > 0)
            {
                bauturi = bauturi.Where(b => b.Price <= FilterPrice.Value).ToList();
            }

            // 4. APLICARE PERMISIUNI DE ȘTERGERE PENTRU AFIȘARE
            if (_currentUser != null)
            {
                foreach (var b in bauturi)
                {
                    b.CanDelete = _currentUser.IsAdmin || b.AddedBy == _currentUser.Username;
                }
            }

            // Actualizăm interfața cu lista filtrată
            ListaBauturi.ItemsSource = bauturi;
        }

        // --- BUTOANE PANOU DE FILTRARE ---

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            // Apelăm funcția care citește filtrele și actualizează lista
            IncarcaDatele();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            // Resetăm interfața de filtre
            FilterCategory.SelectedIndex = 0; // Se întoarce la "Toate"
            FilterLocation.Text = string.Empty;
            FilterPrice.Value = 0;

            // Reîncărcăm datele fără filtre
            IncarcaDatele();
        }


        // --- NAVIGARE UI AUTENTIFICARE ---

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            EroareLogin.Text = string.Empty;
        }

        private void ShowRegister_Click(object sender, RoutedEventArgs e)
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            EroareRegister.Text = string.Empty;
        }

        private void BackToWelcome_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Collapsed;
            WelcomePanel.Visibility = Visibility.Visible;

            LoginUsername.Text = string.Empty;
            LoginPassword.Password = string.Empty;
            RegisterUsername.Text = string.Empty;
            RegisterPassword.Password = string.Empty;
        }


        // --- LOGICĂ LOGIN / REGISTER ---

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Username == LoginUsername.Text && u.Password == LoginPassword.Password);

            if (user != null)
            {
                _currentUser = user;
                AuthGrid.Visibility = Visibility.Collapsed;
                MainAppPanel.Visibility = Visibility.Visible;

                // La prima logare, resetăm filtrele ca să vadă tot
                ClearFilters_Click(null!, null!);
            }
            else
            {
                EroareLogin.Text = "Username sau parolă greșită!";
            }
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RegisterUsername.Text) || string.IsNullOrWhiteSpace(RegisterPassword.Password))
            {
                EroareRegister.Text = "Completează ambele câmpuri pentru a crea contul!";
                EroareRegister.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                return;
            }

            using var db = new AppDbContext();

            if (db.Users.Any(u => u.Username == RegisterUsername.Text))
            {
                var dialog = new ContentDialog
                {
                    Title = "Nume indisponibil",
                    Content = $"Ne pare rău, dar numele de utilizator '{RegisterUsername.Text}' este deja folosit de altcineva.\n\nTe rugăm să alegi un alt nume sau să adaugi o cifră la final.",
                    CloseButtonText = "Am înțeles",
                    XamlRoot = this.Content.XamlRoot
                };

                await dialog.ShowAsync();
                RegisterUsername.Text = string.Empty;
                return;
            }

            var newUser = new User
            {
                Username = RegisterUsername.Text,
                Password = RegisterPassword.Password,
                IsAdmin = false
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            EroareRegister.Text = "Cont creat cu succes! Apasă 'Înapoi' pentru a te loga.";
            EroareRegister.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);

            RegisterUsername.Text = string.Empty;
            RegisterPassword.Password = string.Empty;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            MainAppPanel.Visibility = Visibility.Collapsed;
            AuthGrid.Visibility = Visibility.Visible;
            BackToWelcome_Click(null!, null!);
        }


        // --- ADĂUGARE ȘI ȘTERGERE REVIEW ---

        private void AdaugaReview_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputNumeBautura.Text) && InputCategorie.SelectedItem != null && _currentUser != null)
            {
                using (var db = new AppDbContext())
                {
                    var reviewNou = new Beverage
                    {
                        Name = InputNumeBautura.Text,
                        Category = InputCategorie.SelectedItem.ToString() ?? "Necunoscută",
                        RestaurantLocation = InputRestaurant.Text,
                        Price = InputPret.Value,
                        Rating = (int)InputRating.Value,
                        Comment = InputComentariu.Text,
                        AddedBy = _currentUser.Username
                    };

                    db.Beverages.Add(reviewNou);
                    db.SaveChanges();
                }

                // Curățăm câmpurile de adăugare
                InputNumeBautura.Text = string.Empty;
                InputCategorie.SelectedIndex = -1;
                InputRestaurant.Text = string.Empty;
                InputPret.Value = 0;
                InputRating.Value = 1;
                InputComentariu.Text = string.Empty;

                // După ce am adăugat un element nou, ar trebui să resetăm filtrele pentru a-l putea vedea în listă
                ClearFilters_Click(null!, null!);
            }
        }

        private void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int reviewId && _currentUser != null)
            {
                using (var db = new AppDbContext())
                {
                    var reviewDeSters = db.Beverages.FirstOrDefault(b => b.Id == reviewId);

                    if (reviewDeSters != null)
                    {
                        if (_currentUser.IsAdmin || reviewDeSters.AddedBy == _currentUser.Username)
                        {
                            db.Beverages.Remove(reviewDeSters);
                            db.SaveChanges();
                            IncarcaDatele(); // Reîncărcăm folosind filtrele actuale
                        }
                    }
                }
            }
        }


        // --- OPEN STREET MAP API ---

        private async void InputRestaurant_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text;

                if (query.Length >= 3)
                {
                    try
                    {
                        string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=5";
                        string responseString = await _httpClient.GetStringAsync(url);
                        var results = JsonSerializer.Deserialize<List<OsmPlace>>(responseString);

                        if (results != null && results.Count > 0)
                        {
                            sender.ItemsSource = results.Select(p => p.display_name).ToList();
                        }
                        else
                        {
                            sender.ItemsSource = new List<string> { query };
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Eroare API Hartă: {ex.Message}");
                    }
                }
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }
    }

    public class OsmPlace
    {
        public string display_name { get; set; } = string.Empty;
    }
}