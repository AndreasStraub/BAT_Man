using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BAT_Man.Models;
using BAT_Man.Services;
using BAT_Man.ViewModels;
using BAT_Man.Views;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// Das Haupt-ViewModel ("MainViewModel") für das Hauptfenster.
    /// <para>
    /// ARCHITEKTUR-FUNKTION:
    /// 1. Container: Verwaltet die gesamte Navigation zwischen den Unterseiten (Views).
    /// 2. State-Management: Hält die Instanzen der Unter-ViewModels im Speicher (Caching).
    /// 3. Observer-Subjekt: Benachrichtigt die View (MainWindow.xaml), wenn sich der anzuzeigende Inhalt ändert.
    /// </para>
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // --- Private Felder (Zustandsspeicher) ---

        // Speichert die Daten des aktuell eingeloggten Benutzers für die Anzeige.
        private Teilnehmer _aktuellerTeilnehmer;

        // --- Caching der Unter-ViewModels ---
        private WelcomeViewModel _welcomeViewModel;
        private SettingsViewModel _settingsViewModel;
        private FirmenUebersichtViewModel _firmenUebersichtViewModel;
        private FirmaAnzeigenViewModel _betriebAnzeigenViewModel;

        // Referenziert das ViewModel, das AKTUELL im rechten Bereich (ContentControl) angezeigt wird.
        private object _aktuellesViewModel;

        // Speichert die vorherige Ansicht, um nach dem Schließen der Hilfe dorthin zurückzukehren.
        private object _letzteView;

        // --- Öffentliche Eigenschaften (Datenquellen für die GUI) ---

        /// <summary>
        /// Der Name des angemeldeten Benutzers (für die Anzeige im Menü).
        /// Read-Only (nur get), da er sich nach dem Login nicht mehr ändert.
        /// </summary>
        public string TeilnehmerName { get; }

        /// <summary>
        /// Der Kurs des angemeldeten Benutzers.
        /// </summary>
        public string TeilnehmerKurs { get; }

        /// <summary>
        /// Die Eigenschaft, die bestimmt, welche View gerade angezeigt wird.
        /// Das ContentControl im MainWindow bindet an diese Eigenschaft.
        /// </summary>
        public object AktuellesViewModel
        {
            get { return _aktuellesViewModel; }
            set
            {
                // 1. Wert setzen
                _aktuellesViewModel = value;

                // 2. WICHTIG: Benachrichtigung senden!
                OnPropertyChanged();
            }
        }

        // --- Commands (Die "Fernbedienung" für die Buttons) ---
        // Jede Eigenschaft hier entspricht einem klickbaren Menüpunkt.

        public ICommand ShowWelcomeViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }
        public ICommand ShowFirmaAnlegenCommand { get; }
        public ICommand ShowFirmenUebersichtCommand { get; }
        public ICommand ShowBetriebAnzeigenCommand { get; }
        public ICommand ShowHilfeViewCommand { get; }

        // --- Konstruktor (Initialisierung) ---

        /// <summary>
        /// Wird aufgerufen, sobald das MainWindow erstellt wird (nach erfolgreichem Login).
        /// Lädt Benutzerdaten und initialisiert die Navigation.
        /// </summary>
        public MainWindowViewModel()
        {
            // -------------------------------------------------------
            // SCHRITT 1: Benutzerdaten laden (Singleton-Zugriff)
            // -------------------------------------------------------
            var sitzung = AktiveSitzung.Instance;

            // Sicherheits-Check: Prüfung, ob eine Anmeldung vorliegt.
            if (sitzung.IstAngemeldet())
            {
                _aktuellerTeilnehmer = sitzung.AngemeldeterTeilnehmer;

                TeilnehmerKurs = _aktuellerTeilnehmer.Kurs; 
                TeilnehmerName = $"{_aktuellerTeilnehmer.Vorname} {_aktuellerTeilnehmer.Nachname}";
            }
            else
            {
                // Fallback für den Designer-Modus in Visual Studio
                TeilnehmerKurs = "Nicht angemeldet";
                TeilnehmerName = "Unbekannt";
            }

            // -------------------------------------------------------
            // SCHRITT 2: Unter-ViewModels vorbereiten
            // -------------------------------------------------------
            _welcomeViewModel = new WelcomeViewModel();
            _settingsViewModel = new SettingsViewModel();

            // Übergabe der Referenz auf die eigene Instanz ('this') an das FirmenUebersichtViewModel.
            _firmenUebersichtViewModel = new FirmenUebersichtViewModel(this);

            _betriebAnzeigenViewModel = new FirmaAnzeigenViewModel();

            // -------------------------------------------------------
            // SCHRITT 3: Commands verkabeln
            // -------------------------------------------------------
            // Zuweisung der auszuführenden Methoden an die RelayCommands.
            ShowWelcomeViewCommand = new RelayCommand(ExecuteShowWelcomeView);
            ShowSettingsViewCommand = new RelayCommand(ExecuteShowSettingsView);
            ShowFirmaAnlegenCommand = new RelayCommand(ExecuteShowFirmaAnlegen);
            ShowFirmenUebersichtCommand = new RelayCommand(ExecuteShowFirmenUebersicht);
            ShowBetriebAnzeigenCommand = new RelayCommand(ExecuteShowBetriebAnzeigen);
            ShowHilfeViewCommand = new RelayCommand(ExecuteShowHilfeView);

            // -------------------------------------------------------
            // SCHRITT 4: Startzustand
            // -------------------------------------------------------
            // Initiale Anzeige der Willkommens-Seite.
            AktuellesViewModel = _welcomeViewModel;
        }

        // --- Navigations-Methoden (Die Logik hinter den Klicks) ---

        /// <summary>
        /// Navigiert zur Willkommensseite (Startbildschirm).
        /// Wird ausgeführt, wenn der Benutzer auf "Willkommen" klickt.
        /// </summary>
        /// <param name="p">Parameter vom Command (nicht genutzt).</param>
        private void ExecuteShowWelcomeView(object p)
        {
            AktuellesViewModel = _welcomeViewModel;
        }

        /// <summary>
        /// Navigiert zur Einstellungs-Seite.
        /// Wird ausgeführt, wenn der Benutzer auf "Einstellungen" klickt.
        /// </summary>
        /// <param name="p">Parameter vom Command (nicht genutzt).</param>
        private void ExecuteShowSettingsView(object p)
        {
            AktuellesViewModel = _settingsViewModel;
        }

        /// <summary>
        /// Navigiert zur Firmenübersicht (Tabelle).
        /// Aktualisiert vor der Anzeige die Daten aus der Datenbank.
        /// </summary>
        /// <param name="p">Parameter vom Command (nicht genutzt).</param>
        private void ExecuteShowFirmenUebersicht(object p)
        {
            // WICHTIG: Erneutes Laden der Daten vor dem Umschalten.
            // Stellt sicher, dass zwischenzeitlich hinzugefügte Firmen angezeigt werden.
            _firmenUebersichtViewModel.ExecuteRefresh(null);

            AktuellesViewModel = _firmenUebersichtViewModel;
        }

        /// <summary>
        /// Navigiert zur Detailansicht (Firma anzeigen).
        /// Zeigt die zuletzt ausgewählte Firma an.
        /// </summary>
        /// <param name="p">Parameter vom Command (nicht genutzt).</param>
        private void ExecuteShowBetriebAnzeigen(object p)
        {
            AktuellesViewModel = _betriebAnzeigenViewModel;
        }

        /// <summary>
        /// Navigiert zur Maske "Neue Firma anlegen".
        /// Erstellt dafür explizit eine neue Instanz, um alle Felder zu leeren.
        /// </summary>
        /// <param name="parameter">Wird vom Command ignoriert.</param>
        public void ExecuteShowFirmaAnlegen(object parameter)
        {
            AktuellesViewModel = new FirmaAnlegenViewModel(this, null);
        }

        /// <summary>
        /// Öffnet die kontextsensitive Hilfe und speichert die aktuelle Ansicht.
        /// Ermöglicht es dem Benutzer, nach dem Lesen der Hilfe zur ursprünglichen Seite zurückzukehren.
        /// </summary>
        /// <param name="p">Parameter vom Command (nicht genutzt).</param>
        private void ExecuteShowHilfeView(object p)
        {
            // 1. "Back-Stack" Logik: Speichern der vorherigen Ansicht.
            // Erfolgt nur, wenn die aktuelle Ansicht nicht bereits die Hilfe ist.
            if (!(AktuellesViewModel is HilfeViewModel))
            {
                _letzteView = AktuellesViewModel;
            }

            // 2. Kontext ermitteln (Herkunft des Aufrufs)
            // Auswahl der passenden Hilfeseite basierend auf dem aktiven ViewModel.
            string kontextKey = "Allgemein";

            if (AktuellesViewModel is FirmenUebersichtViewModel)
            {
                kontextKey = "FirmenUebersichtViewModel";
            }
            else if (AktuellesViewModel is FirmaAnzeigenViewModel)
            {
                kontextKey = "FirmaAnzeigenViewModel";
            }
            else if (AktuellesViewModel is FirmaAnlegenViewModel)
            {
                kontextKey = "FirmaAnlegenViewModel";
            }
            else if (AktuellesViewModel is SettingsViewModel)
            {
                kontextKey = "SettingsViewModel";
            }

            // 3. Hilfe-ViewModel erstellen
            var hilfeVM = new HilfeViewModel(kontextKey);

            // 4. "Zurück"-Funktion injizieren (Callback)
            // Übergabe einer Aktion an das Hilfe-VM, die beim Schließen ausgeführt wird.
            hilfeVM.SchliessenCommand = new RelayCommand(o =>
            {
                // Wiederherstellen der alten Ansicht
                if (_letzteView != null)
                {
                    AktuellesViewModel = _letzteView;
                    _letzteView = null; // Reset
                }
                else
                {
                    // Notfall-Fallback
                    AktuellesViewModel = _welcomeViewModel;
                }
            });

            // 5. Anzeigen
            AktuellesViewModel = hilfeVM;
        }

        /// <summary>
        /// Öffentliche Methode für Quernavigation zur Detailansicht.
        /// Wird vom 'FirmenUebersichtViewModel' aufgerufen, wenn dort eine Zeile doppelt geklickt wird.
        /// </summary>
        /// <param name="firma">Die Firma, deren Details angezeigt werden sollen.</param>
        public void NavigateToFirmaDetail(Firma firma)
        {
            // Erstellung eines neuen Detail-ViewModels unter direkter Übergabe der Firma.
            AktuellesViewModel = new FirmaAnzeigenViewModel(firma);
        }

        // --- INotifyPropertyChanged Implementierung ---

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Hilfsmethode, um das PropertyChanged-Event sicher auszulösen.
        /// Informiert die View über Änderungen an Eigenschaften.
        /// </summary>
        /// <param name="propertyName">Der Name der geänderten Eigenschaft (automatisch ermittelt).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}