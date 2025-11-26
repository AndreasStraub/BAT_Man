using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows; // Wichtig für Application.Current
using System.Windows.Input;

namespace WPF_Test.ViewModels
{
    public class LegendenPunkt
    {
        public string Nummer { get; set; }
        public string Beschreibung { get; set; }
    }

    public class HilfeViewModel : INotifyPropertyChanged
    {
        // ... (Properties Titel, BildPfad, Legende wie gehabt) ...
        private string _titel;
        public string Titel { get => _titel; set { _titel = value; OnPropertyChanged(); } }

        private string _bildPfad;
        public string BildPfad { get => _bildPfad; set { _bildPfad = value; OnPropertyChanged(); } }

        public ObservableCollection<LegendenPunkt> Legende { get; set; }
        public ICommand SchliessenCommand { get; set; }

        public HilfeViewModel(string kontextKey)
        {
            Legende = new ObservableCollection<LegendenPunkt>();
            LadeHilfeDaten(kontextKey);
        }

        // [NEU] Hilfsmethode, um Texte aus der Strings.de.xaml zu holen
        private string GetText(string resourceKey)
        {
            var text = Application.Current.TryFindResource(resourceKey) as string;
            return text ?? $"[{resourceKey}]"; // Fallback, falls Key fehlt
        }

        private void LadeHilfeDaten(string key)
        {
            // Sicherheits-Check: Falls die Liste noch Daten hat, leeren.
            Legende.Clear();

            switch (key)
            {
                // --- 1. FIRMEN ÜBERSICHT ---
                case "FirmenUebersichtViewModel":
                    Titel = GetText("Help_Title_Overview");
                    BildPfad = "/WPF_Test;component/Images/Help_FirmenListe.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_Search") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_Table") });
                    Legende.Add(new LegendenPunkt { Nummer = "3", Beschreibung = GetText("Help_Desc_New") });
                    break;

                // --- 2. FIRMEN DETAILS ---
                case "FirmaAnzeigenViewModel":
                    Titel = GetText("Help_Title_Details");
                    BildPfad = "/WPF_Test;component/Images/Help_Details.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_DetailHeader") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_Activities") });
                    break;

                // --- 3. FIRMEN EDITOR (NEU / BEARBEITEN) ---
                case "FirmaAnlegenViewModel":
                    Titel = GetText("Help_Title_Edit");
                    BildPfad = "/WPF_Test;component/Images/Help_Edit.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_Mandatory") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_Save") });
                    Legende.Add(new LegendenPunkt { Nummer = "!", Beschreibung = GetText("Help_Desc_Validation") });
                    break;

                // --- 4. EINSTELLUNGEN ---
                case "SettingsViewModel":
                    Titel = GetText("Help_Title_Settings");
                    BildPfad = "/WPF_Test;component/Images/Help_Settings.png"; // Bild müsstest du noch erstellen

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_Language") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_Theme") });
                    Legende.Add(new LegendenPunkt { Nummer = "3", Beschreibung = GetText("Help_Desc_Password") });
                    break;

                // --- 5. WILLKOMMEN / FALLBACK ---
                case "WelcomeViewModel":
                default:
                    Titel = GetText("Help_Title_Welcome");
                    BildPfad = "/WPF_Test;component/Images/Help_Welcome.png";

                    Legende.Add(new LegendenPunkt { Nummer = "A", Beschreibung = GetText("Help_Desc_Nav") });
                    Legende.Add(new LegendenPunkt { Nummer = "B", Beschreibung = GetText("Help_Desc_Status") });
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}