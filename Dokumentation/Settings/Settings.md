# Modul-Dokumentation: Einstellungen (Settings)

Dieses Modul behandelt die globale Konfiguration der Anwendung, insbesondere Design (Themes), Sprache und Benutzersicherheit.

---

## 1. Dynamische Ressourcen (`DynamicResource`)

In der `SettingsView.xaml` wird für Texte und Farben konsequent `DynamicResource` statt `StaticResource` verwendet.

### Der Unterschied
* **`StaticResource`:** Der Wert wird **einmalig** beim Programmstart (oder Laden des Fensters) abgerufen. Ändert sich die Ressource später (z.B. Farbe von "Rot" auf "Blau"), merkt das Element davon nichts.
* **`DynamicResource`:** Das Element baut eine **permanente Verbindung** zur Ressource auf. Wenn im Hintergrund das Wörterbuch ausgetauscht wird (z.B. Wechsel von `German.xaml` auf `English.xaml`), aktualisiert sich das Textfeld sofort automatisch.

**Beispiel:**
```xml
<TextBlock Text="{DynamicResource Settings_Title}" ... />
```
Wird die Sprache geändert, ändert sich der Wert von `Settings_Title` im Speicher, und der TextBlock zeigt sofort den neuen Text an.

---

## 2. ViewModel-Logik (`SettingsViewModel.cs`)

### 2.1 Theme-Umschaltung
Die Steuerung erfolgt über RadioButtons, die an boolesche Eigenschaften gebunden sind (z.B. `IsDarkMode`).

**Der Ablauf:**
1.  Benutzer klickt auf "Dunkel".
2.  Der Setter von `IsDarkMode` wird aufgerufen (`value = true`).
3.  Das ViewModel ruft den Dienst auf: `ThemeService.ChangeTheme("dark")`.
4.  **Der Dienst:** Entfernt das alte Farb-Wörterbuch aus `App.Current.Resources` und lädt das neue (`DarkTheme.xaml`).
5.  **Die UI:** Dank `DynamicResource` ändern sich sofort alle Farben (Hintergründe, Schriften) in der gesamten Anwendung.

### 2.2 Sprach-Umschaltung
Funktioniert analog zum Theme.
1.  Benutzer klickt auf "Englisch".
2.  Der Setter ruft `LanguageService.Instance.ChangeLanguage("en")`.
3.  Das Wörterbuch `Strings.de.xaml` wird gegen `Strings.en.xaml` getauscht.
4.  Alle Texte ändern sich live.

---

## 3. Passwort-Dialog

Für die Passwort-Änderung wird das bereits in Phase 2 erstellte Fenster `ChangePasswordWindow` wiederverwendet.

```csharp
ChangePasswordWindow passwordWindow = new ChangePasswordWindow();
passwordWindow.Owner = Application.Current.MainWindow;
passwordWindow.ShowDialog();
```

**Wichtig:** Das Setzen des `Owner` (Besitzer) stellt sicher, dass der Dialog immer **über** dem Hauptfenster schwebt und nicht versehentlich dahinter verschwindet. `ShowDialog()` sorgt für Modaliät (Hauptfenster ist blockiert, bis Dialog geschlossen wird).