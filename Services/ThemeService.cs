using System;
using System.Windows;

namespace WPF_Test.Services
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
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                // Wir suchen nach einem Wörterbuch, das entweder:
                // a) Unseren speziellen Marker "ThemeDictIdentifier" enthält (wenn wir schon mal gewechselt haben)
                // b) Oder dessen Quellpfad den Ordner "Themes" enthält (beim ersten Start)
                if (dict.Contains("ThemeDictIdentifier") ||
                    (dict.Source != null && dict.Source.OriginalString.Contains("Themes")))
                {
                    themeDict = dict;
                    break; // Gefunden!
                }
            }

            // Sicherheits-Check: Wenn kein Theme-Dictionary gefunden wurde, abbrechen.
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
            // Damit wir dieses Dictionary beim nächsten Aufruf sicher wiederfinden,
            // setzen wir einen eindeutigen Schlüssel.
            themeDict["ThemeDictIdentifier"] = themeName;
        }
    }
}