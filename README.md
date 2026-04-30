# 🍹 DrinkReview - WinUI 3 Desktop Application

**DrinkReview** este o aplicație modernă pentru Windows (Desktop), construită cu **WinUI 3** și **C#**, care permite utilizatorilor să documenteze, să evalueze și să stocheze fotografii ale băuturilor consumate în diverse locații.

---

## ✨ Funcționalități Principale

*   🔐 **Autentificare Securizată:** Sistem complet de login și înregistrare a utilizatorilor.
*   📸 **Management Fotografii:** Posibilitatea de a încărca poze pentru fiecare recenzie.
*   🔍 **Lightbox Image Viewer:** Click pe orice miniatură din listă pentru a mări fotografia pe tot ecranul.
*   🌍 **Integrare OpenStreetMap:** Sugestii automate de locații (restaurante/cafenele) în timp ce tastezi, folosind API-ul Nominatim.
*   ⭐ **Rating & Topuri:** Sistem de notare cu stele (RatingControl). Posibilitatea de a vedea un "Top al băuturilor" prin sortare după note.
*   🌓 **Temă Dinamică:** Suport complet pentru **Light Mode** și **Dark Mode**, interschimbabile printr-un buton dedicat.
*   🛡️ **Sistem Administrativ:** Cont de administrator pre-configurat pentru gestionarea (ștergerea) oricărui review din baza de date.
*   ⚠️ **Confirmare Ștergere:** Dialog de confirmare (ContentDialog) pentru a preveni ștergerile accidentale.

---

## 🛠️ Tehnologii Utilizate

*   **Framework:** .NET 8 + WinUI 3 (Windows App SDK)
*   **Limbaj:** C# / XAML
*   **Bază de Date:** SQLite (Bază de date locală rapidă)
*   **ORM:** Entity Framework Core (EF Core)
*   **API:** OpenStreetMap (Nominatim) pentru geolocație/autocomplete.



## 🔑 Acces Administrator

Pentru a accesa funcțiile de moderare (ștergerea oricărui review), folosiți următorul cont pre-existent:
*   **Username:** `Admin`
*   **Parolă:** `112233`

---

## 📂 Structura Proiectului

*   `MainWindow.xaml`: Interfața principală a aplicației (XAML).
*   `MainWindow.xaml.cs`: Logica aplicației, gestionarea API-urilor și a interacțiunilor.
*   `Models/`: Definițiile claselor `Beverage` și `User`.
*   `Data/`: Contextul bazei de date `AppDbContext`.

---

Dezvoltat de **Alex Crisan** - [alexcrisan7](https://github.com/alexcrisan7)
