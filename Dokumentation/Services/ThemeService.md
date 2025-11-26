# Modul-Dokumentation: Theme Service

Dieses Modul ermöglicht den Wechsel des Farbschemas ("Skinning") zur Laufzeit.

---

## 1. Funktionsweise: DynamicResources

Das Herzstück des Theming-Systems in WPF ist der Unterschied zwischen `StaticResource` und `DynamicResource`.

### Statisch vs. Dynamisch
* **StaticResource:** Wird einmal beim Laden aufgelöst. Ändert sich der Wert im Hintergrund, bleibt die GUI gleich.
* **DynamicResource:** Baut eine dauerhafte Verbindung auf. Ändert sich der Wert im ResourceDictionary, wird die GUI sofort neu gezeichnet.

In unseren XAML-Dateien (z.B. `BasisStyles.xaml` oder `SettingsView.xaml`) nutzen wir daher konsequent:
```xml
Background="{DynamicResource AppBackground}"
Foreground="{DynamicResource TextForeground}"
```

---

## 2. Der Austausch-Prozess

Der `ThemeService` tauscht nicht einzelne Farben aus, sondern **ganze Wörterbücher**.

1.  **Identifikation:** Der Service sucht in `App.Current.Resources.MergedDictionaries` nach dem Wörterbuch, das für die Farben zuständig ist. Er erkennt es am Pfad (Ordner `/Themes/`) oder an einem zuvor gesetzten Marker (`ThemeDictIdentifier`).
2.  **Wechsel:** Die Eigenschaft `.Source` wird auf eine neue Datei gesetzt (z.B. `Blue.xaml`).
3.  **Reaktion:** WPF entlädt die alten Farben ("Dark") und lädt die neuen ("Blue"). Da die Schlüsselnamen (Keys) in beiden Dateien identisch sind (z.B. `AppBackground`), finden alle `DynamicResource`-Bindungen sofort die neuen Werte.

---

## 3. Unterschied zum LanguageService

Obwohl der Code sehr ähnlich aussieht, gibt es einen feinen Unterschied:

* **LanguageService:** Muss oft **zwei** Dateien austauschen (UI-Texte UND Hilfe-Texte). Daher läuft die Schleife dort immer komplett durch.
* **ThemeService:** Tauscht nur **eine** Datei (die Farben) aus. Sobald diese gefunden wurde (`break`), ist die Arbeit erledigt.