using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WinRT.Interop;
using ReviewBauturi.Models;
using ReviewBauturi.Data;

namespace ReviewBauturi
{
    public sealed partial class MainWindow : Window
    {
        private Models.User? _currentUser;
        private static readonly HttpClient _httpClient = new HttpClient();
        private string _fotografieSelectataPath = string.Empty;

        public MainWindow()
        {
            this.InitializeComponent();
            RootGrid.RequestedTheme = ElementTheme.Light;

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "ReviewBauturiApp_StudentProject");
            }

            try
            {
                using var db = new AppDbContext();
                db.Database.EnsureCreated();

                // === SECURITATE: CREARE CONT ADMIN IMPLICIT ===
                if (!db.Users.Any(u => u.IsAdmin == true))
                {
                    var adminUser = new Models.User
                    {
                        Username = "Admin",
                        Password = "112233",
                        IsAdmin = true
                    };

                    db.Users.Add(adminUser);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EROARE FATALĂ BAZĂ DE DATE: {ex.Message}");
            }
        }

        // ==========================================
        // VIZUALIZARE FOTOGRAFIE MĂRITĂ (LIGHTBOX)
        // ==========================================
        private void Thumbnail_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Image img && img.Tag is BitmapImage bmp)
            {
                EnlargedImage.Source = bmp;
                ImageOverlay.Visibility = Visibility.Visible;
            }
        }

        private void CloseImageOverlay_Click(object sender, RoutedEventArgs e)
        {
            ImageOverlay.Visibility = Visibility.Collapsed;
        }

        private void CloseImageOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageOverlay.Visibility = Visibility.Collapsed;
        }

        // ==========================================
        // TEMA DINAMICĂ (DARK / LIGHT MODE)
        // ==========================================
        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (RootGrid.RequestedTheme == ElementTheme.Light)
            {
                RootGrid.RequestedTheme = ElementTheme.Dark;
                ThemeToggle.Content = "☀️ Light Mode";
            }
            else
            {
                RootGrid.RequestedTheme = ElementTheme.Light;
                ThemeToggle.Content = "🌙 Dark Mode";
            }
        }

        // ==========================================
        // ÎNCĂRCARE, FILTRARE ȘI SORTARE (MULTIPLĂ)
        // ==========================================
        private void IncarcaDatele()
        {
            using var db = new AppDbContext();
            var bauturi = db.Beverages.ToList();

            // 1. FILTRARE CATEGORIE
            if (FilterCategory.SelectedItem != null)
            {
                string categorie = FilterCategory.SelectedItem.ToString() ?? "";
                if (categorie != "Toate")
                {
                    bauturi = bauturi.Where(b => b.Category == categorie).ToList();
                }
            }

            // 2. FILTRARE LOCAȚIE
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

            // 4. SORTARE MULTIPLĂ
            if (FilterSortare.SelectedItem != null)
            {
                switch (FilterSortare.SelectedIndex)
                {
                    case 1: // Top (Cele mai bune note)
                        bauturi = bauturi.OrderByDescending(b => b.Rating).ThenByDescending(b => b.Id).ToList();
                        break;
                    case 2: // Cele mai slabe note
                        bauturi = bauturi.OrderBy(b => b.Rating).ThenByDescending(b => b.Id).ToList();
                        break;
                    case 3: // Preț: Ieftin -> Scump
                        bauturi = bauturi.OrderBy(b => b.Price).ThenByDescending(b => b.Id).ToList();
                        break;
                    case 4: // Preț: Scump -> Ieftin
                        bauturi = bauturi.OrderByDescending(b => b.Price).ThenByDescending(b => b.Id).ToList();
                        break;
                    case 5: // Nume (A - Z)
                        bauturi = bauturi.OrderBy(b => b.Name).ToList();
                        break;
                    case 0: // Cele mai noi (Dată)
                    default:
                        bauturi = bauturi.OrderByDescending(b => b.Id).ToList();
                        break;
                }
            }
            else
            {
                bauturi = bauturi.OrderByDescending(b => b.Id).ToList();
            }

            // 5. APLICARE PERMISIUNI
            if (_currentUser != null)
            {
                foreach (var b in bauturi)
                {
                    b.CanDelete = _currentUser.IsAdmin || b.AddedBy == _currentUser.Username;
                }
            }

            ListaBauturi.ItemsSource = bauturi;
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e) => IncarcaDatele();

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            FilterCategory.SelectedIndex = 0;
            FilterSortare.SelectedIndex = 0;
            FilterLocation.Text = string.Empty;
            FilterPrice.Value = 0.0;
            IncarcaDatele();
        }

        // ==========================================
        // LOGICĂ LOGIN / REGISTER
        // ==========================================
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

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Username == LoginUsername.Text && u.Password == LoginPassword.Password);

            if (user != null)
            {
                _currentUser = user;
                AuthGrid.Visibility = Visibility.Collapsed;
                MainAppPanel.Visibility = Visibility.Visible;
                ClearFilters_Click(null!, null!);
            }
            else
            {
                EroareLogin.Text = "Username sau parolă greșită!";
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RegisterUsername.Text) || string.IsNullOrWhiteSpace(RegisterPassword.Password))
            {
                EroareRegister.Text = "Completează ambele câmpuri!";
                return;
            }

            using var db = new AppDbContext();
            if (db.Users.Any(u => u.Username == RegisterUsername.Text))
            {
                EroareRegister.Text = "Numele este deja luat!";
                return;
            }

            var newUser = new Models.User { Username = RegisterUsername.Text, Password = RegisterPassword.Password, IsAdmin = false };
            db.Users.Add(newUser);
            db.SaveChanges();

            EroareRegister.Text = "Cont creat cu succes! Dă Înapoi pentru a intra.";
            EroareRegister.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
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

        // ==========================================
        // FOTOGRAFII (ADĂUGARE / ȘTERGERE)
        // ==========================================
        private async void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string imagesFolder = Path.Combine(localFolder, "ReviewBauturiImages");
                if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Path);
                string newFilePath = Path.Combine(imagesFolder, newFileName);

                File.Copy(file.Path, newFilePath, true);
                _fotografieSelectataPath = newFilePath;

                PreviewImagine.Source = new BitmapImage(new Uri(newFilePath));
                PreviewImagine.Visibility = Visibility.Visible;
            }
        }

        private void AdaugaReview_Click(object sender, RoutedEventArgs e)
        {
            string mesajEroare = string.Empty;

            // 1. Verificăm rând pe rând pentru a da mesajul corect
            if (string.IsNullOrWhiteSpace(InputNumeBautura.Text) || InputCategorie.SelectedItem == null)
            {
                mesajEroare = "Completează Numele și Categoria!";
            }
            else if (double.IsNaN(InputPret.Value) || InputPret.Value <= 0)
            {
                mesajEroare = "Prețul trebuie să fie mai mare ca 0!";
            }
            else if (InputPret.Value > 99999)
            {
                mesajEroare = "Prețul depășește maximul admis de 99.999 lei!";
            }

            // 2. Dacă avem orice eroare, blocăm salvarea și afișăm mesajul
            if (!string.IsNullOrEmpty(mesajEroare))
            {
                if (sender is Button btn)
                {
                    var continutOriginal = btn.Content;
                    btn.Content = mesajEroare;
                    btn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed);

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                    timer.Tick += (s, args) =>
                    {
                        btn.Content = continutOriginal;
                        btn.ClearValue(Button.BackgroundProperty);
                        timer.Stop();
                    };
                    timer.Start();
                }
                return; // Oprim execuția, NU se salvează în baza de date
            }

            // 3. Dacă totul e perfect, salvăm review-ul
            if (_currentUser != null)
            {
                using (var db = new AppDbContext())
                {
                    var reviewNou = new Models.Beverage
                    {
                        Name = InputNumeBautura.Text,
                        Category = InputCategorie.SelectedItem.ToString() ?? "Necunoscută",
                        RestaurantLocation = InputRestaurant.Text,
                        Price = InputPret.Value,
                        Rating = (int)InputRating.Value,
                        Comment = InputComentariu.Text,
                        AddedBy = _currentUser.Username,
                        ImagePath = _fotografieSelectataPath
                    };

                    db.Beverages.Add(reviewNou);
                    db.SaveChanges();
                }

                // 4. Resetăm câmpurile după salvarea cu succes
                InputNumeBautura.Text = string.Empty;
                InputCategorie.SelectedIndex = -1;
                InputRestaurant.Text = string.Empty;
                InputPret.Value = 0.0;
                InputRating.Value = 1.0;
                InputComentariu.Text = string.Empty;

                _fotografieSelectataPath = string.Empty;
                PreviewImagine.Visibility = Visibility.Collapsed;

                ClearFilters_Click(null!, null!);
            }
        }

        private async void DeleteReview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int reviewId && _currentUser != null)
            {
                // 1. Creăm fereastra de confirmare
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = "Confirmare Ștergere",
                    Content = "Ești sigur că vrei să ștergi definitiv acest review?",
                    PrimaryButtonText = "Da, șterge",
                    CloseButtonText = "Renunță",
                    DefaultButton = ContentDialogButton.Close, // Setăm "Renunță" ca buton implicit pentru siguranță
                    XamlRoot = this.Content.XamlRoot // Obligatoriu în WinUI 3 pentru a ști unde să afișeze dialogul
                };

                // 2. Afișăm fereastra și așteptăm decizia utilizatorului
                var rezultat = await deleteDialog.ShowAsync();

                // 3. Dacă a apăsat "Da, șterge", executăm ștergerea
                if (rezultat == ContentDialogResult.Primary)
                {
                    using (var db = new AppDbContext())
                    {
                        var reviewDeSters = db.Beverages.FirstOrDefault(b => b.Id == reviewId);

                        if (reviewDeSters != null && (_currentUser.IsAdmin || reviewDeSters.AddedBy == _currentUser.Username))
                        {
                            // Ștergem și fișierul fizic al pozei dacă există
                            if (!string.IsNullOrEmpty(reviewDeSters.ImagePath) && File.Exists(reviewDeSters.ImagePath))
                            {
                                try { File.Delete(reviewDeSters.ImagePath); } catch { }
                            }

                            db.Beverages.Remove(reviewDeSters);
                            db.SaveChanges();
                            IncarcaDatele();
                        }
                    }
                }
            }
        }

        // ==========================================
        // OPEN STREET MAP API
        // ==========================================
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
                    catch
                    {
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