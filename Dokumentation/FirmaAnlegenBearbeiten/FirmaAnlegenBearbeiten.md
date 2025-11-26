# Modul-Dokumentation: Firma anlegen & bearbeiten (Dialoge)

Dieses Modul demonstriert die Wiederverwendung von Code (**DRY-Prinzip**) und den Umgang mit Dialogfenstern im MVVM-Pattern.

---

## 1. Konzept: DRY (Don't Repeat Yourself)

Anstatt zwei separate Fenster zu programmieren (eines für "Neue Firma", eines für "Firma bearbeiten"), nutzen wir **eine einzige View** (`FirmaAnlegenView`) und **ein einziges ViewModel** (`FirmaAnlegenViewModel`).

Der Kontext ("Modus") entscheidet über das Verhalten:

| Modus | Auslöser | Verhalten | Daten |
| :--- | :--- | :--- | :--- |
| **Neu** | Klick auf "Neue Firma" (Sidebar) | Wird im Hauptfenster (rechts) angezeigt. | Startet mit leerem Formular. |
| **Edit** | Klick auf "Bearbeiten" (Detailansicht) | Öffnet sich als **Popup-Fenster** (Dialog). | Startet mit den Daten der Firma. |

---

## 2. ViewModel-Logik (`FirmaAnlegenViewModel.cs`)

### 2.1 Unterscheidung im Konstruktor
Der Konstruktor prüft den Parameter `firma`:
* Ist `firma == null` -> **Modus NEU**. Es wird ein neues `new Firma()` Objekt erstellt.
* Ist `firma != null` -> **Modus EDIT**. Es wird eine **Kopie** der Daten erstellt.

**Warum eine Kopie?**
Würden wir das Original bearbeiten, würden Änderungen (z.B. Löschen des Namens) sofort in der Liste im Hintergrund sichtbar sein, noch bevor der Benutzer auf "Speichern" klickt. Durch die Kopie werden Änderungen erst beim Klick auf "Speichern" zurückgeschrieben (`UpdateFirma`).

### 2.2 Steuerung des Fensters (Window Closing)
Im MVVM-Pattern darf das ViewModel das Fenster (`Window`) eigentlich nicht kennen.
Um den Dialog nach dem Speichern trotzdem schließen zu können, nutzen wir einen Trick:

1.  **View:** Der Button übergibt das Fenster als Parameter an den Command:
    ```xml
    CommandParameter="{Binding RelativeSource={RelativeSource AncestorType=Window}}"
    ```
2.  **ViewModel:** Der Command empfängt den Parameter:
    ```csharp
    public void ExecuteSpeichern(object parameter)
    {
        // ... speichern ...
        if (parameter is Window window)
        {
            window.DialogResult = true; // Erfolg melden
            window.Close();             // Fenster schließen
        }
    }
    ```

---

## 3. Exkurs: MVVM vs. Event-Handler

Warum nutzen wir `ICommand` und `Binding` statt einfacher `Click`-Events (`Button_Click`)?

### Das Problem mit "Click"
Code im Code-Behind (`.xaml.cs`) führt zu einer starken Vermischung von Design und Logik ("Spaghetti-Code").
* Wenn sich das Design ändert (z.B. Button wird woanders hin verschoben), geht oft der Code kaputt.
* Man kann die Logik nicht automatisch testen (Unit-Tests), da man dafür immer ein echtes Fenster öffnen müsste.

### Der Vorteil von MVVM
* **Trennung:** Die View (XAML) ist "dumm". Sie weiß nur: "Bei Klick rufe ich Command X auf".
* **Unabhängigkeit:** Das ViewModel (C#) ist "blind". Es kennt die View nicht. Es führt einfach die Logik aus.
* **Testbarkeit:** Wir können `ExecuteSpeichern()` testen, ohne jemals eine Grafikkarte zu benötigen.

Dies entspricht dem **KISS-Prinzip** (Keep It Simple, Stupid) für professionelle Software-Architekturen, da es Wartbarkeit und Sauberkeit langfristig sichert.