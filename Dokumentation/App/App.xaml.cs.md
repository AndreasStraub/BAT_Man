# Dokumentation: App.xaml.cs (Programmstart-Logik)

## Übersicht
Die Klasse `App.xaml.cs` (Code-Behind der App.xaml) beinhaltet die Einstiegslogik der Anwendung. Da in der `App.xaml` die `StartupUri` entfernt wurde, übernimmt die Methode `OnStartup` die vollständige Kontrolle über den Initialisierungsprozess.

## Wichtige Konzepte

### 1. Manueller Startprozess ("Bootstrapping")
Der Begriff "Bootstrapping" bezeichnet in der Softwareentwicklung den Initialisierungsvorgang beim Start einer Anwendung.
* **Standard WPF:** Normalerweise reicht `StartupUri="MainWindow.xaml"`. WPF lädt dann das Fenster automatisch.
* **In dieser Anwendung:** Da vor dem Hauptfenster logische Schritte (Authentifizierung, Sicherheitschecks) nötig sind, muss dieser Startprozess manuell programmiert werden.
* **Der Ablauf:** Die Methode `OnStartup` fungiert als "Bootstrapper". Sie baut die Anwendung schrittweise auf: Erst Login -> Dann Prüfung -> Dann Hauptanwendung. Erst wenn dieser Prozess erfolgreich durchlaufen ist, ist die Anwendung "hochgefahren".

### 2. `ShowDialog()` vs. `Show()`
Dies ist ein entscheidender Unterschied im Ablauf:
* **`ShowDialog()` (Login/Passwort):** Öffnet das Fenster **modal**. Der Code in `OnStartup` hält an dieser Zeile an und wartet ("blockiert"), bis das Fenster geschlossen wird. Erst dann läuft der Code weiter und das Ergebnis (`DialogResult`) kann ausgewertet werden.
* **`Show()` (MainWindow):** Öffnet das Fenster **nicht-modal**. Der Code läuft sofort weiter. Dies ist notwendig für das Hauptfenster, damit die `OnStartup`-Methode beendet werden kann und die Anwendung in den normalen Ereignis-Loop übergeht.

### 3. Verbindung von View und ViewModel (Wiring)
Da keine Dependency Injection Container (wie Autofac oder Microsoft DI) verwendet werden, geschieht die Verknüpfung hier manuell:
`view.DataContext = viewModel;`
Dies weist die View an, ihre Daten aus dem erstellten ViewModel zu beziehen.

## Exkurs: C# Syntax-Besonderheiten & Patterns

### Das Schlüsselwort `partial`
Die Klasse ist definiert als `public partial class App`.
* **Bedeutung:** Das Schlüsselwort signalisiert, dass der Quellcode dieser Klasse auf mehrere Dateien verteilt ist.
* **Hintergrund:** Visual Studio generiert aus der XAML-Datei (`App.xaml`) automatisch eine versteckte C#-Datei (`App.g.cs`), die technische Initialisierungen übernimmt. Durch `partial` können der selbstgeschriebene Code (`App.xaml.cs`) und der generierte Code sauber getrennt bleiben und werden beim Kompilieren verschmolzen.

### Nullable Types (`bool?`)
In der Zeile `bool? loginResult = loginWindow.ShowDialog();` wird ein Datentyp mit einem Fragezeichen verwendet.
* **Problem:** Ein normaler `bool` kann nur `true` oder `false` sein.
* **Lösung:** Ein `Nullable<bool>` (kurz `bool?`) erweitert dies auf drei Zustände:
    1.  `true` (z.B. User klickte "OK" / Login erfolgreich).
    2.  `false` (z.B. User klickte "Abbrechen").
    3.  `null` (User hat das Fenster über das "X" geschlossen, ohne Entscheidung).

### Das Singleton-Pattern (`AktiveSitzung.Instance`)
In der Zeile `var aktuellerUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;` wird auf ein globales Objekt zugegriffen.
* **Das Konzept:** Ein Singleton ist eine Klasse, von der es zur Laufzeit nur **genau eine einzige Instanz** geben darf. Es stellt einen globalen Zugriffspunkt bereit.
* **Warum hier?** Es gibt in der Anwendung nur einen angemeldeten Benutzer. Dieser Status muss von überall (Login, Main, Settings) erreichbar sein, ohne dass das User-Objekt ständig von Methode zu Methode herumgereicht werden muss.
* **Syntax:** Über die statische Eigenschaft `.Instance` greift man auf dieses "Einzelstück" zu.

## Programmablauf (Debug-Sicht)

Der folgende Ablauf beschreibt den Call-Stack beim Starten der Anwendung:

1.  **Start:** `OnStartup(StartupEventArgs e)` wird vom Framework aufgerufen.
2.  **Phase 1 - Authentifizierung:**
    * `LoginViewModel` und `LoginWindow` werden erstellt.
    * `loginWindow.ShowDialog()` wird aufgerufen -> **Der Code wartet hier.**
    * *User interagiert mit Login-Fenster...*
    * Fenster schließt. `ShowDialog()` liefert `true` (Erfolg) oder `false`/`null` (Abbruch).
    * Bei Misserfolg: `Shutdown()` -> App beendet sich sofort.
3.  **Phase 2 - Sicherheitsprüfung:**
    * Zugriff auf `AktiveSitzung.Instance` (Singleton-Zugriff auf User-Daten).
    * Prüfung: `MussPasswortAendern`.
    * Falls `true`: `ChangePasswordWindow` wird erstellt und modal (`ShowDialog`) angezeigt.
    * Bei Abbruch: `Shutdown()`.
4.  **Phase 3 - Hauptanwendung:**
    * Erst jetzt wird `MainWindowViewModel` erstellt (lädt z.B. Daten aus der DB).
    * `MainWindow` wird erstellt und verknüpft.
    * `mainWindow.Show()` macht das Fenster sichtbar.
5.  **Ende:** Die Methode `OnStartup` ist fertig. Die App läuft weiter, solange das MainWindow offen ist (wegen `OnExplicitShutdown` Logik, die intern vom Framework verwaltet wird, sobald ein Fenster offen ist).