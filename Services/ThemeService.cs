using System;
using System.Windows;

namespace BAT_Man.Services
{
    /// <summary>
    /// Verwaltet das visuelle Design (Theme) der Anwendung.
    /// <para>
    /// FUNKTIONALITÄT:
    /// Tauscht zur Laufzeit die Farb-Ressourcen aus (z.B. Dark/Light/Blue).
    /// Da in der Anwendung DynamicResources verwendet werden, ändern sich alle Farben sofort.
    /// </para>
    /// </summary>
    public static class ThemeService
    {
        /// <summary>
        /// Ändert das Farbschema der gesamten Anwendung.
        /// </summary>
        /// <param name="themeName">Der Name des Themes ("light", "dark", "blue", "green").</param>
        public static void ChangeTheme(string themeName)
        {
            ResourceDictionary themeDict = null;

            // 1. Suche nach dem aktuellen Theme-Wörterbuch in der App.xaml
            // Die Anwendung besitzt eine Liste von "MergedDictionaries" (zusammengeführte Ressourcen).
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                // Es wird nach einem Wörterbuch gesucht, das entweder:
                // a) Den speziellen Marker "ThemeDictIdentifier" enthält (falls bereits gewechselt wurde)
                // b) Oder dessen Quellpfad den Ordner "Themes" enthält (beim ersten Start)
                if (dict.Contains("ThemeDictIdentifier") ||
                    (dict.Source != null && dict.Source.OriginalString.Contains("Themes")))
                {
                    themeDict = dict;
                    break; // Wörterbuch gefunden.
                }
            }

            // Sicherheits-Check: Wenn kein Theme-Dictionary identifiziert wurde, wird abgebrochen.
            if (themeDict == null) return;

            // 2. Zieldatei bestimmen
            string sourceFile;
            switch (themeName.ToLower())
            {
                case "light": sourceFile = "/Themes/Light.xaml"; break;
                case "blue": sourceFile = "/Themes/Blue.xaml"; break;
                case "green": sourceFile = "/Themes/Green.xaml"; break;

                case "dark":
                default: sourceFile = "/Themes/Dark.xaml"; break;
            }

            // 3. Austausch durchführen
            // Durch das Setzen der Source-Eigenschaft lädt WPF die neuen Ressourcen.
            // Alle DynamicResources in der GUI (z.B. {DynamicResource AppBackground}) aktualisieren sich sofort.
            themeDict.Source = new Uri(sourceFile, UriKind.Relative);

            // 4. Marker setzen
            // Um dieses Dictionary beim nächsten Aufruf sicher wiederzufinden,
            // wird ein eindeutiger Schlüssel gesetzt.
            themeDict["ThemeDictIdentifier"] = themeName;
        }
    }
}