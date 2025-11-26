# Modul-Dokumentation: Firmenübersicht

Dieses Modul behandelt die Darstellung von Datenlisten und die Interaktion mit ausgewählten Datensätzen per Maus-Gesten.

---

## 1. Exkurs: Listen in WPF (`ObservableCollection` vs. `List`)

Ein häufiger Anfängerfehler in WPF ist die Verwendung der Standard-Liste `List<T>` für die Datenbindung.

### Das Problem mit `List<T>`
Die Standard-Liste (`List<string>`, `List<Firma>`) ist "dumm".
* Wenn Sie im C#-Code `meineListe.Add(neueFirma)` aufrufen, werden die Daten im Speicher zwar aktualisiert.
* **Aber:** Die GUI (das `ListView`) bekommt davon nichts mit. Die Anzeige bleibt alt.

### Die Lösung: `ObservableCollection<T>`
WPF stellt eine spezielle Listen-Klasse bereit: `System.Collections.ObjectModel.ObservableCollection<T>`.
* Sie funktioniert fast genauso wie eine normale Liste.
* **Der Unterschied:** Sie implementiert intern das `INotifyCollectionChanged`-Interface.
* **Der Effekt:** Wenn Sie `.Add()` oder `.Remove()` aufrufen, "feuert" die Liste ein Event. Die GUI hört dieses Event und aktualisiert automatisch die Anzeige.

**Merksatz:**
> Für reine interne Berechnungen nehmen wir `List<T>`.
> Sobald eine Liste in der GUI angezeigt werden soll (`ItemsSource`), nehmen wir **immer** `ObservableCollection<T>`.

---

## 2. Analyse der View (`FirmenUebersichtView.xaml`)

Zur Darstellung der Daten wird ein `ListView` in Kombination mit einem `GridView` verwendet. Dies erzeugt eine klassische Tabellenansicht.

### 2.1 Datenbindung (`ItemsSource`)
```xml
<ListView ItemsSource="{Binding FirmenListe}" ...>
```
Hier wird die `ObservableCollection` aus dem ViewModel an die Liste gebunden.

### 2.2 Auswahl (`SelectedItem`)
```xml
<ListView SelectedItem="{Binding AusgewaehlteFirma, Mode=TwoWay}" ...>
```
* **Zweck:** Synchronisation der Markierung.
* **Richtung GUI -> Code:** Klickt der User eine Zeile an, landet das Firmen-Objekt in der Eigenschaft `AusgewaehlteFirma` im ViewModel.

### 2.3 Die "versteckte" Interaktion (Style & Templates)

In der View-Datei selbst ist kein Code für den Doppelklick zu finden. Trotzdem funktioniert er.
Der Grund liegt im **zentralen Style** für `ListViewItem` (definiert in `App.xaml` oder ResourceDictionary).

Dort findet sich folgende Definition im `ControlTemplate`:

```xml
<MouseBinding MouseAction="LeftDoubleClick"
              Command="{Binding DataContext.BearbeitenCommand, 
                       RelativeSource={RelativeSource AncestorType=ListView}}"/>
```

**Erklärung der Technik (`RelativeSource`):**
Ein einzelnes Listenelement (`ListViewItem`) kennt normalerweise nur sein eigenes Datenobjekt (die Firma). Es weiß nicht, dass es ein `BearbeitenCommand` im ViewModel gibt.

Mit `RelativeSource` "sucht" das Element im visuellen Baum nach oben:
1.  **AncestorType=ListView:** "Suche das übergeordnete Element vom Typ ListView."
2.  **Path=DataContext.BearbeitenCommand:** "Nimm von dieser ListView den DataContext (das ist das `FirmenUebersichtViewModel`) und rufe dort `BearbeitenCommand` auf."

**Vorteil:**
Diese Logik muss nur einmal programmiert werden und gilt dann automatisch für **alle** Listen in der gesamten Anwendung, die diesen Style verwenden.

---

## 3. Analyse des ViewModels (`FirmenUebersichtViewModel.cs`)

### 3.1 Dependency Injection (Navigation)
Der Konstruktor nimmt das `MainWindowViewModel` entgegen. Dadurch kann die Firmenliste Navigationsbefehle an das Hauptfenster senden (`_mainVm.NavigateToFirmaDetail(...)`).

### 3.2 Daten laden (`ExecuteRefresh`)
Die Methode `ExecuteRefresh` zeigt den typischen Umgang mit der `ObservableCollection`:
1.  **Leeren:** `FirmenListe.Clear()` – Die GUI wird sofort leer.
2.  **Füllen:** `FirmenListe.Add(...)` – Mit jedem Hinzufügen erscheint sofort eine neue Zeile in der Tabelle.