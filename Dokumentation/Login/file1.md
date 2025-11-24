# Modul-Dokumentation: Login & Authentifizierung

Diese Dokumentation beschreibt die Umsetzung des Login-Prozesses unter Verwendung des MVVM-Musters und des Singleton-Patterns.

---

## 1. Ablaufdiagramm
Den schematischen Ablauf finden Sie hier:
[Ablaufdiagramm öffnen](LoginProzess.drawio)

Der Prozess folgt dem Schema:
1.  **View:** Benutzer gibt Daten ein und betätigt "Anmelden".
2.  **ViewModel:** Empfängt den Befehl und ruft den `AuthenticationService` auf.
3.  **Service:** Prüft Anmeldedaten gegen die Datenbank (Repository).
4.  **Singleton:** Bei Erfolg wird der User in `AktiveSitzung` gespeichert.
5.  **App.xaml.cs:** Erhält das Ergebnis und entscheidet über den Programmstart.

---

## 2. ViewModel & Commands (`LoginViewModel.cs`)

Im Gegensatz zur klassischen Ereignissteuerung (`Click_Event` im Code-Behind) kommen hier **Commands** zum Einsatz.

### ICommand
* **Definition:** Eine Schnittstelle, die Aktionen kapselt.
* **Zweck:** Entkopplung des Auslösers (Button) von der Logik (Methode). Der Button muss nicht wissen, *was* passiert, nur *dass* ein Befehl ausgeführt wird.

### RelayCommand
Da `ICommand` ein Interface ist, wird eine Hilfsklasse `RelayCommand` verwendet (oft in einem `Core`- oder `Helper`-Ordner), die die Methoden `Execute` (Was soll passieren?) und `CanExecute` (Darf der Button geklickt werden?) an das ViewModel weiterleitet.

---

## 3. Singleton Pattern (`AktiveSitzung.cs`)

Das Singleton-Muster stellt sicher, dass eine Klasse nur genau **einmal** instanziiert wird.

### Implementierung
```csharp
// 1. Private statische Variable (der Speicher)
private static AktiveSitzung _instance;

// 2. Privater Konstruktor (Verhindert "new AktiveSitzung()")
private AktiveSitzung() { ... }

// 3. Öffentlicher Zugriff (Die Schleuse)
public static AktiveSitzung Instance
{
    get
    {
        if (_instance == null)
            _instance = new AktiveSitzung(); // Erstellen nur beim allerersten Mal
        return _instance;
    }
}

Testkommentar
