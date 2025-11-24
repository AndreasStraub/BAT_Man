// Dateipfad: Services/ThemeService.cs

using System;
using System.Windows;

namespace WPF_Test.Services
{
    public static class ThemeService
    {
        public static void ChangeTheme(string themeName)
        {
            ResourceDictionary themeDict = null;

            // 1. Das richtige Wörterbuch finden
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                // ==========================================================
                // KORREKTUR: Wir suchen jetzt auch nach dem PFAD.
                // Das fängt den Fall ab, dass beim ersten Start noch kein
                // "ThemeDictIdentifier" gesetzt ist.
                // ==========================================================
                if (dict.Contains("ThemeDictIdentifier") ||
                   (dict.Source != null && dict.Source.OriginalString.Contains("Themes")))
                {
                    themeDict = dict;
                    break;
                }
            }

            // Wenn immer noch nichts gefunden wurde, brich ab (sollte nicht passieren)
            if (themeDict == null) return;

            // 2. Pfad bestimmen (Ihr Switch-Code)
            string sourceFile;
            switch (themeName.ToLower())
            {
                case "light": sourceFile = "/Themes/Light.xaml"; break;
                case "blue": sourceFile = "/Themes/Blue.xaml"; break;
                case "green": sourceFile = "/Themes/Green.xaml"; break;
                case "dark":
                default: sourceFile = "/Themes/Dark.xaml"; break;
            }

            // 3. Austauschen
            themeDict.Source = new Uri(sourceFile, UriKind.Relative);

            // 4. Marker setzen (damit wir es beim nächsten Mal schneller finden)
            themeDict["ThemeDictIdentifier"] = themeName;
        }
    }
}