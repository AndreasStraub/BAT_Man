# Modul-Dokumentation: RelayCommand (Helper)

Dieses Modul stellt die technische Infrastruktur bereit, um Benutzeroberfläche (View) und Logik (ViewModel) über **Commands** zu verbinden. Es ist eine Standard-Komponente in fast jeder MVVM-Anwendung.

---

## 1. Problemstellung: Das ICommand Interface

WPF-Steuerelemente (wie `Button` oder `MenuItem`) besitzen eine Eigenschaft `Command`. Diese erwartet ein Objekt, das die Schnittstelle `System.Windows.Input.ICommand` implementiert.

### Warum eine eigene Klasse?
Das Interface `ICommand` schreibt vor, dass die Methoden `Execute` und `CanExecute` vorhanden sein müssen.
* Man *könnte* für jeden Button eine eigene Klasse schreiben (z.B. `LoginCommand`, `SpeichernCommand`, `LoeschenCommand`).
* **Nachteil:** Das würde zu hunderten kleinen Klassen führen.

### Die Lösung: RelayCommand
Der `RelayCommand` ist eine generische Klasse. Sie "leitet weiter" (to relay).
Anstatt die Logik fest einzubauen, bekommt der `RelayCommand` die Logik erst zur Laufzeit (im Konstruktor des ViewModels) als **Methodenzeiger (Delegate)** übergeben.

So kann eine einzige Klasse (`RelayCommand`) für alle Buttons in der gesamten Anwendung genutzt werden.

---

## 2. Exkurs: Der CommandManager ("Die Magie")

Ein entscheidendes Detail im Code ist die Implementierung des Events `CanExecuteChanged`:

```csharp
public event EventHandler CanExecuteChanged
{
    add { CommandManager.RequerySuggested += value; }
    remove { CommandManager.RequerySuggested -= value; }
}
```

### Das Problem der Aktualisierung
Stellen Sie sich einen "Speichern"-Button vor, der nur aktiv sein darf, wenn Text in einer TextBox steht.
1.  Der Benutzer tippt ein Zeichen.
2.  Die Bedingung `CanExecute` ändert sich von `false` auf `true`.
3.  **Aber:** Der Button weiß nicht, dass er seine Gültigkeit neu prüfen muss. Er bleibt grau.

### Die Lösung: Der CommandManager
Der `CommandManager` ist ein statischer Dienst in WPF. Er überwacht die gesamte Anwendung auf Benutzerinteraktionen (Mausklicks, Tastatureingaben, Fokuswechsel).

* Jedes Mal, wenn der Benutzer *irgendetwas* tut, löst der CommandManager das Ereignis `RequerySuggested` aus ("Es wird vorgeschlagen, den Status neu abzufragen").
* Durch die oben gezeigte Code-Zeile hängt sich unser `RelayCommand` an diesen globalen Taktgeber.
* **Ergebnis:** Sobald der Benutzer tippt oder klickt, werden **alle** Buttons in der Oberfläche gezwungen, ihre `CanExecute`-Methode neu aufzurufen. Der "Speichern"-Button wird also automatisch aktiv, sobald validier Text eingegeben wurde.

---

## 3. Verwendung im ViewModel

Die Nutzung folgt immer dem gleichen Muster:

1.  **Deklaration:**
    ```csharp
    public ICommand MeinBefehl { get; }
    ```
2.  **Initialisierung (im Konstruktor):**
    ```csharp
    // 1. Parameter: Was tun? (Execute)
    // 2. Parameter: Darf ich? (CanExecute) - optional
    MeinBefehl = new RelayCommand(ExecuteMeineLogik, CanExecuteMeineLogik);
    ```
3.  **Methoden:**
    ```csharp
    private void ExecuteMeineLogik(object parameter) { ... }
    private bool CanExecuteMeineLogik(object parameter) { return ...; }
    ```