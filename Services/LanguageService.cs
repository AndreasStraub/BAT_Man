// Dateipfad: Services/LanguageService.cs

using System;
using System.Windows;

namespace WPF_Test.Services
{
    public class LanguageService
    {
        public static LanguageService Instance { get; } = new LanguageService();
        public string AktuelleSprache { get; private set; }

        private LanguageService()
        {
            AktuelleSprache = "de";
        }

        public void ChangeLanguage(string languageCode)
        {
            // 1. Code normalisieren (Kleinschreibung)
            languageCode = languageCode.ToLower();
            this.AktuelleSprache = languageCode;

            // 2. Das richtige Wörterbuch finden (Robuste Such-Logik)
            ResourceDictionary sprachDict = null;
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                // Wir suchen nach dem Marker ODER dem Pfad "Languages"
                if (dict.Contains("SprachDictIdentifier") ||
                   (dict.Source != null && dict.Source.OriginalString.Contains("Languages")))
                {
                    sprachDict = dict;
                    break;
                }
            }

            // Sicherheits-Check
            if (sprachDict == null) return;

            // ==========================================================
            // 3. NEU: Switch-Statement für einfache Erweiterbarkeit
            // ==========================================================
            string sourceFile;
            switch (languageCode)
            {
                case "en":
                    sourceFile = "/Languages/English.xaml";
                    break;

                // Platz für weitere Sprachen:
                // case "fr": 
                //     sourceFile = "/Languages/French.xaml"; 
                //     break;

                case "de":
                default:
                    // Fallback auf Deutsch, wenn Code unbekannt
                    sourceFile = "/Languages/German.xaml";
                    break;
            }
            // ==========================================================

            // 4. Austauschen
            sprachDict.Source = new Uri(sourceFile, UriKind.Relative);

            // 5. Marker aktualisieren
            sprachDict["SprachDictIdentifier"] = languageCode;
        }
    }
}