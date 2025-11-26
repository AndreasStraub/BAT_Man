# Modul-Dokumentation: Language Service

Dieses Modul ermöglicht den Wechsel der Anzeigesprache zur Laufzeit ("Live-Umschaltung"). Es arbeitet eng mit dem Ressourcensystem von WPF zusammen.

---

## 1. Funktionsweise: Resource Dictionaries

In der Datei `App.xaml` werden verschiedene Wörterbücher geladen (z.B. Styles, Farben, Sprachen). Diese befinden sich in der Liste `Application.Current.Resources.MergedDictionaries`.

### Das Prinzip des Austauschs
Um die Sprache zu ändern, wird das alte Sprach-Wörterbuch nicht entfernt und ein neues hinzugefügt (was die Reihenfolge durcheinanderbringen könnte), sondern die **Quelle (`Source`)** des bestehenden Wörterbuchs wird geändert.

1.  **Suche:** Der Service iteriert durch alle geladenen Wörterbücher.
2.  **Identifikation:** Er prüft anhand des Dateipfads (`OriginalString`), um welche Art von Datei es sich handelt (z.B. enthält "/Languages/").
3.  **Austausch:** Die Eigenschaft `.Source` wird auf den Pfad der neuen Sprache gesetzt (z.B. von `German.xaml` auf `English.xaml`).

### Der Effekt auf die GUI
Da in den Views `DynamicResource` verwendet wird (z.B. `{DynamicResource Settings_Title}`), bemerkt WPF, dass sich das Wörterbuch geändert hat. Die Oberfläche lädt sofort den neuen Text für den Schlüssel `Settings_Title` nach.

---

## 2. Verwaltung mehrerer Dateien

Der Service unterscheidet zwischen zwei Arten von Sprachdateien:

1.  **UI-Strings (`/Languages/`):** Kurze Texte für Buttons, Labels und Menüs.
2.  **Hilfe-Texte (`/Help/`):** Längere Erklärungstexte oder Dokumentationen, die in der Anwendung angezeigt werden.

Durch die `if/else if`-Logik in der Schleife wird sichergestellt, dass immer die korrekte Datei gegen ihr passendes Gegenstück ausgetauscht wird (Deutsch-UI gegen Englisch-UI, Deutsch-Hilfe gegen Englisch-Hilfe).

---

## 3. Integration mit der Datenbank

Der `LanguageService` speichert den aktuellen Sprachcode (z.B. "de") in der Eigenschaft `AktuelleSprache`.

Diese Eigenschaft ist essenziell für die **Repositories** (z.B. `StatusRepository` oder `AktivitaetRepository`). Wenn Daten aus der Datenbank geladen werden, greifen die Repositories auf diesen Service zu, um dem SQL-Server mitzuteilen, in welcher Sprache die Stammdaten (z.B. Status-Bezeichnungen) geliefert werden sollen.