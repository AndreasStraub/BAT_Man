# Modul-Dokumentation: Hauptfenster (MainWindow)

Dieses Modul beschreibt das zentrale Fenster der Anwendung. Es dient als "Container", der verschiedene Ansichten (Views) dynamisch lädt und die Navigation bereitstellt.

---

## 1. Architektur-Konzept: Single Window Application

In modernen WPF-Anwendungen wird häufig nur ein einziges Hauptfenster (`MainWindow`) verwendet. Der Inhalt dieses Fensters ändert sich dynamisch, anstatt dass neue Fenster geöffnet werden.

### Das Prinzip:
1.  **Der Rahmen:** Das `MainWindow` enthält das Menü (links) und einen Platzhalter (rechts).
2.  **Der Platzhalter:** Ein `ContentControl` dient als Leinwand.
3.  **Der Inhalt:** Das `MainViewModel` besitzt eine Eigenschaft `AktuellesViewModel`.
4.  **Das Binding:** Wird `AktuellesViewModel` geändert, tauscht WPF automatisch die Ansicht im Platzhalter aus.

---

## 2. Technische Umsetzung

### 2.1 Das Layout (`Grid`)
Die grafische Oberfläche wird durch ein `Grid` unterteilt:
* **Spalte 0 (Links):** Navigationsleiste mit Buttons.
* **Spalte 1 (Rechts):** `ContentControl` für den wechselnden Inhalt.

### 2.2 Visuelles Feedback
Damit der Benutzer erkennt, welche Seite aktiv ist, wird der entsprechende Navigations-Button hervorgehoben (siehe Kapitel 4).

---

## 3. Exkurs: Der DataContext

In der Datei `MainWindow.xaml` findet sich zu Beginn folgender Block:

```xml
<Window.DataContext>
    <viewmodels:MainWindowViewModel/>
</Window.DataContext>
```

Dies ist einer der wichtigsten Schritte im MVVM-Pattern. Hier wird die Verbindung zwischen der grafischen Oberfläche (View) und der Logik (ViewModel) hergestellt.

### 3.1 Funktion als "Daten-Topf"
WPF-Bindings funktionieren wie Suchanfragen. Wenn im XAML `{Binding TeilnehmerName}` steht, sucht das Fenster nach einer Eigenschaft mit diesem Namen.
Der `DataContext` definiert, **wo** gesucht wird.

---

## 4. Detail-Analyse: Dynamische Farben & Converter

Im XAML-Code der Navigationsbuttons findet sich ein komplexer `DataTrigger` mit einem `Converter`.

### 4.1 Die Frage: Warum nicht einfach "Background=Rot"?
Wenn man im XAML direkt `Background="Red"` schreibt, ist der Button **immer** rot.
Wir wollen aber ein **Tab-Verhalten**:
* Ist die Seite "Willkommen" aktiv? -> Button "Willkommen" ist farbig, alle anderen grau.
* Ist die Seite "Firmen" aktiv? -> Button "Firmen" ist farbig, "Willkommen" wird grau.

Das Aussehen muss also **dynamisch** auf den Zustand der Anwendung reagieren.

### 4.2 Die Lösung: Der DataTrigger (Wenn-Dann-Regel)
Ein `DataTrigger` erlaubt es, Eigenschaften nur dann zu ändern, wenn eine bestimmte Bedingung erfüllt ist.
Die Logik lautet:
*"WENN (AktuellesViewModel == Meine Seite) DANN (Setze Background = AppBackground)"*

### 4.3 Das Problem beim Vergleich (Hier kommt der Converter ins Spiel)
Der Trigger muss prüfen, ob das im ViewModel gespeicherte Objekt zur Button-Seite passt.

* **Im ViewModel (`AktuellesViewModel`):** Dort liegt eine **Instanz** (ein konkretes Speicherobjekt, z.B. `new WelcomeViewModel()`).
* **Im XAML (`x:Type`):** Der Button weiß nur, für welche **Klasse** (Typ) er zuständig ist (z.B. `WelcomeViewModel`).

Der **InstanceToTypeConverter** löst das Problem: Er schaut sich das Objekt an und liefert dessen Typ zurück.

---

## 5. Theorie: Das Observer Pattern



[Image of Observer Pattern Diagram]


Das in `MainWindowViewModel.cs` verwendete Prinzip basiert auf dem **Observer Pattern** (Beobachter-Muster). Dies ist ein Standard-Entwurfsmuster in der Softwareentwicklung.

### 5.1 Die Rollenverteilung
Das Muster definiert eine 1-zu-n-Abhängigkeit zwischen Objekten:

1.  **Das Subjekt (Subject):**
    * In unserem Fall: Das `MainWindowViewModel`.
    * Aufgabe: Es verwaltet Daten (z.B. `AktuellesViewModel`). Wenn sich diese Daten ändern, benachrichtigt es alle Interessenten.
2.  **Der Beobachter (Observer):**
    * In unserem Fall: Die View (`MainWindow.xaml`).
    * Aufgabe: Er "abonniert" das Subjekt. Sobald er eine Benachrichtigung erhält, aktualisiert er seine Anzeige.

### 5.2 Implementierung in C#
In .NET wird dieses Muster durch das Interface `INotifyPropertyChanged` umgesetzt.

Der Ablauf im Code (siehe Eigenschaft `AktuellesViewModel`):

```csharp
set 
{ 
    // 1. Änderung des Zustands (Das Subjekt ändert sich)
    _aktuellesViewModel = value; 
    
    // 2. Benachrichtigung (Das Subjekt ruft die Beobachter)
    OnPropertyChanged(); 
}
```

### 5.3 Analogie: Der Newsletter
* **Subjekt:** Ein Nachrichten-Portal (ViewModel).
* **Beobachter:** Die Abonnenten (View).
* **OnPropertyChanged:** Die E-Mail "Breaking News!".
* **Ablauf:** Das Portal veröffentlicht einen neuen Artikel (Variable ändert sich) und schickt eine Mail raus. Die Abonnenten lesen die Mail und wissen nun Bescheid. Ohne die Mail (Notification) würden die Abonnenten nicht wissen, dass es etwas Neues gibt.

---

## 6. ViewModel-Logik

Das `MainWindowViewModel` steuert den gesamten Ablauf.

### 6.1 Initialisierung
Im Konstruktor werden folgende Schritte ausgeführt:
1.  **Sitzungsdaten laden:** Über das Singleton `AktiveSitzung` werden Name und Kurs des eingeloggten Benutzers abgerufen.
2.  **Sub-ViewModels erstellen:** Instanzen für die Unterseiten werden vorbereitet.
3.  **Referenzübergabe:** Dem `FirmenUebersichtViewModel` wird eine Referenz auf das Haupt-ViewModel (`this`) übergeben. Dies ermöglicht der Unterseite, Navigationsbefehle an das Hauptfenster zu senden.

### 6.2 Navigations-Methoden
Die Navigation erfolgt durch das Setzen der Eigenschaft `AktuellesViewModel`.
Dies löst das `PropertyChanged`-Event aus, woraufhin das `ContentControl` im Hauptfenster den Inhalt austauscht.