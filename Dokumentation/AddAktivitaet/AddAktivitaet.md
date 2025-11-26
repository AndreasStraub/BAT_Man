# Modul-Dokumentation: Aktivitäten-Dialog

Dieses Modul beschreibt den Dialog zum Erstellen und Bearbeiten von Aktivitäten (Historien-Einträge).

---

## 1. Konzept: Einer für alles (DRY)

Analog zum Firmen-Dialog wird **eine einzige View** (`AddAktivitaetDialog`) und **ein ViewModel** (`AddAktivitaetViewModel`) für zwei verschiedene Zwecke verwendet:

1.  **Neu:** Der Aufruf erfolgt mit `null`. Das Datum wird auf "Heute" gesetzt, ein Standard-Status gewählt und die Eingabefelder bleiben leer.
2.  **Bearbeiten:** Der Aufruf erfolgt mit einer existierenden `Aktivitaet`. Die Felder werden mit den bestehenden Daten gefüllt, und der "Löschen"-Button wird sichtbar.

---

## 2. Besonderheiten der Steuerelemente

### 2.1 DatePicker (Datumsauswahl)
```xml
<DatePicker SelectedDate="{Binding AktivitaetZumBearbeiten.Datum, Mode=TwoWay}"/>
```
WPF stellt mit dem `DatePicker` ein komplexes Steuerelement bereit, das Validierung und ein Kalender-Popup automatisch mitbringt. Es wird direkt an eine `DateTime`-Eigenschaft gebunden.

### 2.2 Status-Auswahl (ListBox als RadioButtons)
Es soll dem Benutzer ermöglicht werden, *einen* Status auszuwählen. Technisch handelt es sich um eine Liste, optisch soll dies jedoch durch RadioButtons dargestellt werden.

**Die Umsetzung im XAML:**
Es wird eine reguläre `ListBox` verwendet, deren Aussehen jedoch über einen Style (`RadioListBoxStyle`, definiert in `BasisStyles.xaml`) modifiziert wird.
* Jedes Listenelement (`ListBoxItem`) enthält einen `RadioButton`.
* Die Eigenschaft `IsChecked` des RadioButtons ist an `IsSelected` des Listenelements gebunden.
* **Ergebnis:** Die Optik entspricht RadioButtons, während technisch die Vorteile der Listen-Bindung (`ItemsSource`) genutzt werden.

---

## 3. ViewModel-Logik (`AddAktivitaetViewModel.cs`)

### 3.1 Die Status-Suche
Beim Bearbeiten einer Aktivität muss der passende Status in der Liste gefunden und markiert werden.
Da mit Objekten gearbeitet wird (Status-Objekte in der Datenbank vs. Status-Objekte im Speicher), reicht ein einfacher Referenz-Vergleich oft nicht aus. Daher erfolgt der Vergleich über die **IDs**:

```csharp
// Klassische Suche (didaktisch wertvoll):
foreach (Status s in AlleStatusOptionen)
{
    if (s.Status_ID == gesuchteID)
    {
        AusgewaehlterStatus = s;
        break;
    }
}
```

### 3.2 Dynamischer Fenstertitel
Der Titel des Fensters passt sich automatisch dem Kontext an:
* Modus Neu: "Aktivität hinzufügen"
* Modus Edit: "Aktivität bearbeiten"

Dies wird rein über XAML (`Style.Triggers`) gelöst, ohne C#-Code im Code-Behind:
```xml
<DataTrigger Binding="{Binding IsEditMode}" Value="True">
    <Setter Property="Title" Value="{DynamicResource EditActivityDialog_Title}"/>
</DataTrigger>
```