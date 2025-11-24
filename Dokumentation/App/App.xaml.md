# Dokumentation: App.xaml

## Übersicht
Die `App.xaml` fungiert als definitorischer Einstiegspunkt der WPF-Anwendung. Es wird keine Benutzeroberfläche definiert, sondern es werden globale Ressourcen und Konfigurationen festgelegt, die während der gesamten Laufzeit der Anwendung Gültigkeit besitzen.

## Wichtige Konzepte

### 1. ShutdownMode="OnExplicitShutdown"
* **Standard:** `OnMainWindowClose` (Beendung der Anwendung beim Schließen des Hauptfensters).
* **Problemstellung:** Da der Startprozess ein Login-Fenster beinhaltet, welches vor dem Öffnen des Hauptfensters geschlossen wird, würde der Standard-Modus zu einer verfrühten Beendung der Anwendung führen.
* **Lösung:** `OnExplicitShutdown`. Das Beenden der Anwendung wird explizit im Code-Behind (`App.xaml.cs`) durch den Aufruf von `Application.Current.Shutdown()` gesteuert.

### 2. ResourceDictionary & MergedDictionaries
Die `<Application.Resources>` dienen als globaler Speicher für Design-Ressourcen. Zur Wahrung der Übersichtlichkeit und Wartbarkeit werden Definitionen in externe Dateien ausgelagert:
* **BasisStyles.xaml:** Grundlegende Stile für Steuerelemente (Buttons, Textboxen).
* **Dark.xaml:** Farbdefinitionen (Themes).
* **German.xaml:** Lokalisierung (Texte).
* **InstanceToTypeConverter:** Ein global verfügbarer Converter, der für die Menü-Logik benötigt wird.

### 3. DataTemplates (MVVM Navigation)
Aufgrund der strikten Anwendung des MVVM-Musters (ohne Code-Behind in den Views) ist eine Zuordnung von ViewModels zu Views erforderlich.
* **Mapping:** Der Datentyp `DataType="{x:Type vm:WelcomeViewModel}"` wird der View `<v:WelcomeView/>` zugeordnet.
* **Funktion:** Sobald die Anwendung zur Laufzeit auf ein ViewModel trifft (z.B. in einem ContentControl), wird basierend auf diesen Templates automatisch die zugehörige View instanziiert.
* **Vorteil:** Es besteht eine lose Kopplung; das ViewModel agiert unabhängig von der visuellen Repräsentation.

## Exkurs: XAML-Syntax (`x:`, `Key`, `Type`)

Zum Verständnis der `App.xaml` sind folgende XAML-spezifische Anweisungen essentiell:

### Der Namensraum `xmlns:x`
Ganz oben in der Datei wird definiert: `xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"`.
* **Bedeutung:** Das `x` steht für den XAML-Compiler selbst. Während normale Tags (wie `<Button>`) WPF-Steuerelemente darstellen, sind Befehle mit `x:` Anweisungen an den Parser, *wie* der Code zu interpretieren ist.

### `x:Key` (Der eindeutige Schlüssel)
* **Verwendung:** `<converters:InstanceToTypeConverter x:Key="InstanceToTypeConverter"/>`
* **Erklärung:** Ressourcen in einem `ResourceDictionary` benötigen einen Namen, um gefunden zu werden. `x:Key` ist vergleichbar mit einem Variablennamen in C#.
* **Funktion:** Über diesen Schlüssel kann später mittels `{StaticResource InstanceToTypeConverter}` auf das Objekt zugegriffen werden.

### `x:Type` (Der Typ-Verweis)
* **Verwendung:** `DataType="{x:Type viewmodels:WelcomeViewModel}"`
* **Erklärung:** Hier wird nicht eine *Instanz* (ein konkretes Objekt) eines ViewModels übergeben, sondern die *Klassen-Definition* selbst.
* **Analogie zu C#:** Entspricht dem Operator `typeof(WelcomeViewModel)`.
* **Zweck:** WPF benötigt diese Information, um zu prüfen: "Ist das Objekt, das ich gerade anzeigen soll, vom Typ `WelcomeViewModel`?" Falls ja, wird das definierte Template angewendet.

## Programmablauf (Initialisierung)
1.  Start der Anwendung (Main-Methode).
2.  Parsing der `App.xaml`.
3.  Laden aller `MergedDictionaries` in den Arbeitsspeicher.
4.  Registrierung der `DataTemplate`-Regeln (ohne sofortige Instanziierung der Views).
5.  Übergabe der Kontrolle an `App.xaml.cs` (OnStartup).