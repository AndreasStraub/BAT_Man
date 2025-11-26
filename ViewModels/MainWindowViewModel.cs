// Dateipfad: ViewModels/MainWindowViewModel.cs
// REPARIERTE VERSION (Setzt 'AktuellesViewModel' statt 'ShowDialog')

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WPF_Test.Models;
using WPF_Test.Services;
using WPF_Test.ViewModels;
using WPF_Test.Views;

namespace WPF_Test.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // --- Private Felder ---
        private Teilnehmer _aktuellerTeilnehmer;
        private WelcomeViewModel _welcomeViewModel;
        private SettingsViewModel _settingsViewModel;

        // Wir brauchen das ViewModel hier nicht mehr,
        // da wir es bei jedem Klick neu erstellen (siehe unten).
        // private FirmaAnlegenViewModel _firmaAnlegenViewModel; 

        private FirmenUebersichtViewModel _firmenUebersichtViewModel;
        private FirmaAnzeigenViewModel _betriebAnzeigenViewModel;
        private object _aktuellesViewModel;

        // --- Öffentliche Eigenschaften (Properties) ---
        public string TeilnehmerKurs { get; }
        public string TeilnehmerName { get; }
        public object AktuellesViewModel
        {
            get { return _aktuellesViewModel; }
            set { _aktuellesViewModel = value; OnPropertyChanged(); }
        }

        // --- Befehle (Commands) ---
        public ICommand ShowWelcomeViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }
        public ICommand ShowFirmaAnlegenCommand { get; }
        public ICommand ShowFirmenUebersichtCommand { get; }
        public ICommand ShowBetriebAnzeigenCommand { get; }
        public ICommand OpenHelpPdfCommand { get; }



        /// <summary>
        /// Konstruktor
        /// </summary>
        public MainWindowViewModel()
        {
            // --- Teilnehmer-Daten laden ---
            var sitzung = AktiveSitzung.Instance;
            if (sitzung.IstAngemeldet())
            {
                _aktuellerTeilnehmer = sitzung.AngemeldeterTeilnehmer;
                TeilnehmerKurs = _aktuellerTeilnehmer.Kurs;
                TeilnehmerName = $"{_aktuellerTeilnehmer.Vorname} {_aktuellerTeilnehmer.Nachname}";
            }
            else
            {
                TeilnehmerKurs = "Nicht angemeldet";
                TeilnehmerName = "Unbekannt";
            }

            // --- Seiten-Gehirne instanziieren ---
            _welcomeViewModel = new WelcomeViewModel();
            _settingsViewModel = new SettingsViewModel();
            _firmenUebersichtViewModel = new FirmenUebersichtViewModel(this); // "this" übergibt das Hauptfenster-Gehirn
            _betriebAnzeigenViewModel = new FirmaAnzeigenViewModel();

            // --- Commands zuweisen ---
            ShowWelcomeViewCommand = new RelayCommand(ExecuteShowWelcomeView);
            ShowSettingsViewCommand = new RelayCommand(ExecuteShowSettingsView);
            ShowFirmaAnlegenCommand = new RelayCommand(ExecuteShowFirmaAnlegen);
            ShowFirmenUebersichtCommand = new RelayCommand(ExecuteShowFirmenUebersicht);
            ShowBetriebAnzeigenCommand = new RelayCommand(ExecuteShowBetriebAnzeigen);
           


            // --- Standard-Seite festlegen ---
            AktuellesViewModel = _welcomeViewModel;
        }

        // --- Ausführungs-Methoden ---
        private void ExecuteShowWelcomeView(object p) { AktuellesViewModel = _welcomeViewModel; }
        private void ExecuteShowSettingsView(object p) { AktuellesViewModel = _settingsViewModel; }

        private void ExecuteShowFirmenUebersicht(object p)
        {
            _firmenUebersichtViewModel.ExecuteRefresh(null);
            AktuellesViewModel = _firmenUebersichtViewModel;
        }
        private void ExecuteShowBetriebAnzeigen(object p)
        {
            AktuellesViewModel = _betriebAnzeigenViewModel;
        }

        // ==========================================================
        // HIER IST DIE REPARATUR
        // ==========================================================
        public void ExecuteShowFirmaAnlegen(object parameter)
        {
            // Wir übergeben 'this' (uns selbst) und 'null' (für neue Firma)
            AktuellesViewModel = new FirmaAnlegenViewModel(this, null);
        }

        /// <summary>
        /// Öffentlicher Navigations-Befehl, der von anderen
        /// ViewModels aufgerufen werden kann.
        /// </summary>
        public void NavigateToFirmaDetail(Firma firma)
        {
            // Erstellt das Ziel-ViewModel mit der ausgewählten Firma
            // und setzt es als aktuelle Seite.
            AktuellesViewModel = new FirmaAnzeigenViewModel(firma);
        }

        // --- INotifyPropertyChanged Implementierung ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }











    }
}