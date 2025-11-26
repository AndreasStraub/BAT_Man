# Modul-Dokumentation: Firmen-Detailansicht

Dieses Modul beschreibt die Anzeige einer einzelnen Firma inklusive ihrer Aktivitäten (Historie) und Bearbeitungsfunktionen.

---

## 1. Exkurs: Warum MVVM statt "Click"-Events? (KISS-Prinzip)

Ein häufiger Einwand von Anfängern ist: *"Warum ist der Code so kompliziert mit `ICommand` und `Binding`? Ein einfaches `Button_Click` im Code-Behind wäre doch viel schneller programmiert!"*

### Das Missverständnis
Das "Click"-Verfahren ist nur für die ersten 5 Minuten "einfach". Bei wachsenden Projekten führt es schnell zu Problemen:
1.  **Spaghetti-Code:** UI-Logik und Geschäftslogik vermischen sich im Code-Behind.
2.  **Keine Testbarkeit:** Man kann einen Button-Klick kaum automatisiert testen.
3.  **Hohe Kopplung:** Wenn man das Fensterdesign ändert, muss man oft auch den C#-Code umschreiben.

### Der MVVM-Weg (echtes KISS)
MVVM trennt Verantwortlichkeiten sauber:
* **Die View (Gesicht):** Ist "dumm". Sie sendet nur Signale (z.B. `Command="{Binding LoeschenCommand}"`), weiß aber nicht, was danach passiert.
* **Das ViewModel (Gehirn):** Ist "intelligent". Es empfängt Signale und verarbeitet sie.
* **Vorteil:** Man kann das "Gehirn" testen, ohne ein Fenster öffnen zu müssen. Das Design kann sich ändern, ohne dass die Logik kaputtgeht.

---

## 2. Analyse der View (`FirmaAnzeigenView.xaml`)

Die View ist in zwei Bereiche unterteilt, die durch `GroupBox`-Elemente strukturiert werden.

### 2.1 Navigation & Auswahl (`ComboBox`)
```xml
<ComboBox ItemsSource="{Binding AlleFirmen}"
          SelectedItem="{Binding AusgewaehlteFirma, Mode=TwoWay}" ... />
```
Diese ComboBox ist das zentrale Steuerelement.
* Ändert der User die Auswahl, lädt das ViewModel die neuen Daten.
* Navigiert der User aus der Listenansicht hierher, setzt das ViewModel den Wert, und die ComboBox springt automatisch auf die richtige Firma.

### 2.2 Aktivitäten-Liste (`ListView`)
Im unteren Bereich werden alle Kontakt-Historien (Anrufe, Notizen) angezeigt. Auch hier greift der globale Style für den Doppelklick, der den `BearbeitenCommand` auslöst.

---

## 3. Analyse des ViewModels (`FirmaAnzeigenViewModel.cs`)

### 3.1 Konstruktor-Überladung (Navigation)
Das ViewModel besitzt zwei Konstruktoren:
1.  **Standard (`public FirmaAnzeigenViewModel()`):** Wird genutzt, wenn die Seite "leer" über das Menü geöffnet wird.
2.  **Mit Parameter (`public FirmaAnzeigenViewModel(Firma firmaToSelect)`):** Wird genutzt, wenn von der Übersicht navigiert wird.

**Technik:** Der parametrisierte Konstruktor ruft mit `: this()` zuerst den Standard-Konstruktor auf (um Listen zu laden) und setzt dann die Auswahl.

### 3.2 Kaskadierende Aktualisierung
```csharp
public Firma AusgewaehlteFirma
{
    set
    {
        _ausgewaehlteFirma = value;
        OnPropertyChanged();
        LadeAktivitaeten(); // <--- Automatische Folge-Aktion
    }
}
```
Sobald eine neue Firma gesetzt wird (egal ob durch Klick in der ComboBox oder durch Navigation), wird automatisch `LadeAktivitaeten()` aufgerufen. Dies stellt sicher, dass die untere Liste immer zur oberen Auswahl passt.

### 3.3 Lösch-Logik mit Sicherheitsnetz
Der `FirmaLoeschenCommand` enthält eine Sicherheitsabfrage (`MessageBox`).
Erst wenn der User "Ja" klickt, wird das Repository aufgerufen. Dies verhindert versehentlichen Datenverlust.