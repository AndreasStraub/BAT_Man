# Projektdokumentation: Architektur & Konzepte (BAT-Man)

## 1. Einführung und Zielsetzung
Die Anwendung "BAT-Man" (Bewerbungs-Aktivitäts-Management) ist eine WPF-Desktop-Anwendung zur Verwaltung von Bewerbungsprozessen. Ziel der Architektur ist eine strikte Trennung von Verantwortlichkeiten, um Wartbarkeit, Erweiterbarkeit und Testbarkeit zu gewährleisten.

## 2. Das Architektur-Muster: MVVM (Model-View-ViewModel)
Das fundamentale Design-Pattern dieser Anwendung ist **MVVM**. Es dient der Entkopplung der grafischen Benutzeroberfläche (GUI) von der Geschäftslogik und den Daten.

> **[Diagramm-Platzhalter: Das MVVM-Dreieck]**
> *Darstellung der Beziehungen:*
> * **View:** Kennt nur das ViewModel (via DataBinding). Kennt nicht das Model.
> * **ViewModel:** Kennt das Model. Kennt nicht die View.
> * **Model:** Unabhängige Datenklasse.

### 2.1 Die Schichten im Detail

#### View (Die Ansicht)
* **Bestandteile:** XAML-Dateien (Fenster, UserControls).
* **Verantwortlichkeit:** Definition des visuellen Layouts und Designs.
* **Logik:** Enthält keinen prozeduralen Code im Code-Behind (`.cs`). Interaktionen erfolgen ausschließlich über `DataBinding` und `Commands`.

#### ViewModel (Der Vermittler)
* **Bestandteile:** Klassen im Namespace `ViewModels`.
* **Verantwortlichkeit:** Aufbereitung von Daten für die View und Kapselung der Präsentationslogik.
* **Funktion:** Stellt Eigenschaften (`Properties`) und Befehle (`ICommand`) bereit, an die sich die View bindet. Es steuert den Status der Oberfläche (z.B. Ladebalken, Fehlermeldungen).

#### Model (Die Daten)
* **Bestandteile:** Reine Datenklassen (POCOs) wie `Teilnehmer`, `Bewerbung`.
* **Verantwortlichkeit:** Repräsentation der Geschäftsobjekte und Datenstrukturen, unabhängig von der Darstellung.

## 3. Datenhaltung & Zugriff (Repository Pattern)
Der Datenzugriff erfolgt nicht direkt aus der Präsentationsschicht. Zur Abstraktion der Datenbankzugriffe wird das **Repository Pattern** eingesetzt.

> **[Diagramm-Platzhalter: Der Datenfluss]**
> *Ablauf:* ViewModel -> Repository -> MySQL Datenbank

### 3.1 Funktionsweise
* **Datenbank:** MySQL (lokal) speichert die persistenten Informationen.
* **Repository:** Spezifische Klassen (z.B. `FirmenRepository`) kapseln die SQL-Logik.
* **Ablauf:** Das ViewModel fordert Daten an (z.B. `GetAll()`). Das Repository führt die SQL-Abfrage aus, mappt die Ergebnisse auf Model-Objekte und gibt diese zurück. Dies ermöglicht einen späteren Austausch der Datenbanktechnologie ohne Änderung der UI-Logik.

## 4. Globale Daten & Zustand (Singleton Pattern)
Für Informationen, die während der gesamten Laufzeit anwendungsweit verfügbar sein müssen (z.B. der aktuell angemeldete Benutzer), wird das **Singleton Pattern** verwendet.

* **Klasse:** `AktiveSitzung`
* **Prinzip:** Es wird technisch sichergestellt, dass zur Laufzeit exakt eine Instanz dieser Klasse existiert.
* **Zugriff:** Über die statische Eigenschaft `.Instance` kann von jeder Komponente (Login, Einstellungen, Speichervorgänge) auf den globalen Status zugegriffen werden.

## 5. Programmablauf & Navigation

### 5.1 Der Start-Prozess ("Bootstrapping")
Die Anwendung verfügt über keine statische `StartupUri`. Der Startvorgang wird manuell in `App.xaml.cs` gesteuert ("Guard Pattern").

> **[Diagramm-Platzhalter: Flussdiagramm Start]**
> *Start -> Login (modal) -> Erfolg? -> Passwort-Check (modal) -> Main Window (nicht-modal)*

1.  **Initialisierung:** Laden der Ressourcen (Styles, Sprachen).
2.  **Authentifizierung:** Modales Anzeigen des Login-Fensters.
3.  **Validierung:** Prüfung der Anmeldedaten gegen die Datenbank.
4.  **Sicherheitsrichtlinie:** Prüfung auf Passwort-Änderungszwang.
5.  **Hauptanwendung:** Erst nach erfolgreichem Durchlauf aller Schritte wird das `MainWindow` instanziiert.

### 5.2 Aufbau des Hauptfensters (UI-Composition)
Das `MainWindow` dient als Rahmen (Shell). Die Navigation erfolgt nicht fensterbasiert, sondern durch Austausch von Inhalten (`ContentControl`).

* **Mechanismus:** DataTemplates (definiert in `App.xaml`).
* **Ablauf:**
    1.  Ein `CurrentViewModel` wird im Haupt-ViewModel gesetzt.
    2.  Das `ContentControl` bindet an dieses ViewModel.
    3.  WPF sucht das passende `DataTemplate` (Mapping von VM-Typ zu View-Typ).
    4.  Die entsprechende View wird automatisch gerendert.

## 6. Beispiel eines Ablaufs (Sequenz)
Exemplarischer Ablauf "Speichern einer neuen Firma":

1.  **User:** Betätigt den Button "Speichern" in der UI.
2.  **View:** Das Binding löst den `SaveCommand` im `FirmaAnlegenViewModel` aus.
3.  **ViewModel:**
    * Validiert die Eingaben.
    * Liest die ID des Erstellers aus `AktiveSitzung.Instance`.
    * Ruft `FirmenRepository.Add(neueFirma)` auf.
4.  **Repository:** Generiert den `INSERT`-SQL-Befehl und sendet ihn an die Datenbank.
5.  **Datenbank:** Persistiert den Datensatz.