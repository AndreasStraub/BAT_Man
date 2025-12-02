using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows; 
using System.Windows.Input;

namespace BAT_Man.ViewModels
{
    public class LegendenPunkt
    {
        public string Nummer { get; set; }
        public string Beschreibung { get; set; }
    }

    public class HilfeViewModel : INotifyPropertyChanged
    {
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
                    BildPfad = "/Images/FirmenUebersicht.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_Table") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_CurrentStatus") });

                    break;

                // --- 2. FIRMEN DETAILS ---
                case "FirmaAnzeigenViewModel":
                    Titel = GetText("Help_Title_Details");
                    BildPfad = "/Images/FirmaAnzeigen.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_Choose") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_ButtonsEdit_Delete") });
                    Legende.Add(new LegendenPunkt { Nummer = "3", Beschreibung = GetText("Help_Desc_StatusList") });
                    Legende.Add(new LegendenPunkt { Nummer = "4", Beschreibung = GetText("Help_Desc_ButtonActivities") });
                    break;

                // --- 3. FIRMEN EDITOR (NEU / BEARBEITEN) ---
                case "FirmaAnlegenViewModel":
                    Titel = GetText("Help_Title_Add");
                    BildPfad = "/Images/FirmaAnlegen.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_AddCompany") });

                    break;

                // --- 4. EINSTELLUNGEN ---
                case "SettingsViewModel":
                    Titel = GetText("Help_Title_Settings");
                    BildPfad = "/BAT_Man;component/Images/Einstellungen.png"; 

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_ChangeDesign") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_ChangeLanguage") });
                    Legende.Add(new LegendenPunkt { Nummer = "3", Beschreibung = GetText("Help_Desc_ChangePassword") });

                    break;

                // --- 5. WILLKOMMEN / FALLBACK ---
                case "WelcomeViewModel":
                default:
                    Titel = GetText("Help_Title_Welcome");
                    BildPfad = "/BAT_Man;component/Images/WillkommensSeite.png";

                    Legende.Add(new LegendenPunkt { Nummer = "1", Beschreibung = GetText("Help_Desc_Overview") });
                    Legende.Add(new LegendenPunkt { Nummer = "2", Beschreibung = GetText("Help_Desc_Add") });
                    Legende.Add(new LegendenPunkt { Nummer = "3", Beschreibung = GetText("Help_Desc_Details") });
                    Legende.Add(new LegendenPunkt { Nummer = "4", Beschreibung = GetText("Help_Desc_Settings") });
                    Legende.Add(new LegendenPunkt { Nummer = "5", Beschreibung = GetText("Help_Desc_Help") });
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