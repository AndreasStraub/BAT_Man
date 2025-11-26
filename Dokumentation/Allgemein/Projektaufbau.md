# Projekt-Dokumentation: Architektur & Struktur

Diese Übersicht beschreibt den Aufbau der Projektmappe im Visual Studio. Die Struktur folgt streng dem **MVVM-Muster** (Model-View-ViewModel) und trennt die Datenbanklogik durch das **Repository-Pattern** ab.

---

## 1. Die Ordnerstruktur (Überblick)

Die Dateien sind nicht zufällig verteilt, sondern nach ihrer **technischen Verantwortung** sortiert.

```text
WPF_Test (Projekt)
│
├── 📁 Models           (Die Daten-Struktur)
│   ├── Firma.cs
│   ├── Aktivitaet.cs
│   └── ...
│
├── 📁 Views            (Das Gesicht / Die GUI)
│   ├── MainWindow.xaml
│   ├── LoginWindow.xaml
│   ├── FirmenUebersichtView.xaml
│   └── ...
│
├── 📁 ViewModels       (Das Gehirn / Die Logik)
│   ├── MainWindowViewModel.cs
│   ├── LoginViewModel.cs
│   ├── FirmenUebersichtViewModel.cs
│   └── ...
│
├── 📁 Services         (Globale Helfer)
│   ├── AktiveSitzung.cs
│   ├── LanguageService.cs
│   ├── ThemeService.cs
│   └── RelayCommand.cs
│
├── 📁 Repositories     (Der Datenbank-Zugriff)
│   ├── BaseRepository.cs
│   ├── FirmaRepository.cs
│   └── ...
│
├── 📁 Resources        (Design & Sprache)
│   ├── 📁 Images
│   ├── 📁 Languages    (German.xaml, English.xaml)
│   ├── 📁 Themes       (Dark.xaml, Light.xaml)
│   └── BasisStyles.xaml
│
└── App.xaml            (Startpunkt & Ressourcen-Verwaltung)
```

---

## 2. Die Schichten im Detail

### 2.1 Models (Die Daten)
Hier liegen reine C#-Klassen (POCOs - Plain Old CLR Objects).
* **Zweck:** Sie definieren nur, *wie* ein Datensatz aussieht (welche Eigenschaften er hat).
* **Regel:** Keine Logik, kein Datenbank-Code, keine GUI-Elemente.
* *Beispiel:* "Eine Firma hat einen Namen, eine Straße und eine ID."

### 2.2 Views (Die Ansicht)
Hier liegen die XAML-Dateien (Fenster und UserControls).
* **Zweck:** Definition des Aussehens (Layout, Farben, Buttons).
* **Regel:** Der Code-Behind (`.xaml.cs`) sollte so leer wie möglich sein. Keine Geschäftslogik!
* *Verbindung:* Die View kennt ihr ViewModel über den `DataContext`.

### 2.3 ViewModels (Die Steuerung)
Hier liegt die eigentliche Anwendungslogik.
* **Zweck:** Sie holen Daten aus den Repositories, bereiten sie auf (z.B. für Listen) und stellen Befehle (`ICommand`) für die Buttons bereit.
* **Regel:** Ein ViewModel darf **niemals** direkt auf UI-Elemente (wie `TextBox` oder `Button`) zugreifen. Die Kommunikation läuft nur über **DataBinding** und **INotifyPropertyChanged**.

### 2.4 Services (Die Infrastruktur)
Hier liegen Klassen, die überall in der App gebraucht werden (Singleton-Dienste).
* **AktiveSitzung:** Wer ist eingeloggt?
* **Language/ThemeService:** Wie sieht die App aus?
* **RelayCommand:** Technischer Helfer für Buttons.

### 2.5 Repositories (Die Datenbank)
Hier – und nur hier – findet SQL-Code statt.
* **Zweck:** Übersetzung zwischen C#-Objekten (Models) und der MySQL-Datenbank.
* **BaseRepository:** Verwaltet die Verbindung (`MySqlConnection`).
* **Fach-Repositories:** Enthalten Methoden wie `GetAlleFirmen()`, `SaveFirma()`, `DeleteFirma()`.

---

## 3. Der Datenfluss (Wie alles zusammenspielt)

Wenn ein Benutzer eine Liste von Firmen sehen möchte, passiert Folgendes durch die Schichten hindurch:

1.  **View:** Benutzer öffnet `FirmenUebersichtView`.
2.  **ViewModel:** Der Konstruktor des `FirmenUebersichtViewModel` startet.
3.  **Repository:** Das ViewModel ruft `_firmaRepository.GetAlleFirmen()` auf.
4.  **Datenbank:** Das Repository sendet `SELECT * FROM...` an MySQL.
5.  **Model:** Das Repository verpackt die SQL-Antwort in `Firma`-Objekte (Models) und gibt eine Liste zurück.
6.  **ViewModel:** Speichert die Liste in einer `ObservableCollection`.
7.  **View:** Das DataBinding bemerkt die Änderung und zeigt die Daten in der `ListView` an.

Dieser klare Weg (Separation of Concerns) sorgt dafür, dass Fehler leicht gefunden und Teile der Software (z.B. die Datenbank) ausgetauscht werden können, ohne den Rest zu zerstören.