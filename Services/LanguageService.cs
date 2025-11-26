using System;
using System.Windows;

namespace WPF_Test.Services
{
    /// <summary>
    /// Verwaltet die Lokalisierung (Spracheinstellung) der Anwendung.
    /// <para>
    /// FUNKTIONALITÄT:
    /// Tauscht zur Laufzeit die ResourceDictionaries (Wörterbücher) aus, 
    /// sodass sich Texte in der GUI sofort aktualisieren.
    /// </para>
    /// </summary>
    public class LanguageService
    {
        // --- Singleton Implementierung ---

        /// <summary>
        /// Die einzige Instanz des Services (Singleton).
        /// </summary>
        public static LanguageService Instance { get; } = new LanguageService();

        /// <summary>
        /// Privater Konstruktor verhindert Instanziierung von außen.
        /// Setzt die Standardsprache beim Start.
        /// </summary>
        private LanguageService()
        {
            AktuelleSprache = "de";
        }

        // --- Eigenschaften ---

        /// <summary>
        /// Der Code der aktuell aktiven Sprache (z.B. "de", "en").
        /// Wird von Repositories benötigt, um sprachabhängige Daten aus der DB zu laden.
        /// </summary>
        public string AktuelleSprache { get; private set; }

        // --- Logik ---

        /// <summary>
        /// Ändert die Sprache der gesamten Anwendung.
        /// </summary>
        /// <param name="languageCode">Der Ziel-Sprachcode ("de" oder "en").</param>
        public void ChangeLanguage(string languageCode)
        {
            languageCode = languageCode.ToLower();
            this.AktuelleSprache = languageCode;

            // 1. Pfade zu den neuen Ressourcendateien definieren
            string uiFile;
            string helpFile;

            switch (languageCode)
            {
                case "en":
                    uiFile = "/Languages/English.xaml";
                    helpFile = "/Help/HelpEnglish.xaml";
                    break;

                case "de":
                default:
                    uiFile = "/Languages/German.xaml";
                    helpFile = "/Help/HelpGerman.xaml";
                    break;
            }

            // 2. Die aktiven Wörterbücher der Anwendung durchsuchen und austauschen.
            // Die Schleife iteriert über alle in App.xaml eingebundenen Ressourcen.
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict.Source == null) continue;

                string aktuellerPfad = dict.Source.OriginalString;

                // --- FALL A: Austausch der GUI-Texte ---
                // Identifikation anhand des Ordnernamens "/Languages/" im Pfad.
                if (aktuellerPfad.Contains("/Languages/"))
                {
                    // Die Quelle (Source) wird auf die neue Datei umgebogen.
                    // WPF lädt daraufhin die neuen Strings, und alle DynamicResources aktualisieren sich.
                    dict.Source = new Uri(uiFile, UriKind.Relative);

                    // Optional: Setzen eines Markers, falls dieser Dictionary-Eintrag später
                    // spezifisch identifiziert werden muss.
                    dict["SprachDictIdentifier"] = languageCode;
                }

                // --- FALL B: Austausch der Hilfe-Texte ---
                // Identifikation anhand des Ordnernamens "/Help/" im Pfad.
                else if (aktuellerPfad.Contains("/Help/"))
                {
                    dict.Source = new Uri(helpFile, UriKind.Relative);
                }

                // HINWEIS: Die Schleife läuft weiter, da potentiell beide Dateien (UI und Hilfe)
                // gefunden und ersetzt werden müssen.
            }
        }
    }
}