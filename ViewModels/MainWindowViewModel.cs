using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BAT_Man.Models;
using BAT_Man.Services;
using System.Windows.Input;
using BAT_Man.ViewModels;
using BAT_Man.Views;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// Das Haupt-ViewModel ("MainViewModel").
    /// <para>
    /// ARCHITEKTUR-HINWEIS:
    /// Diese Klasse fungiert im Observer-Pattern als "Subjekt" (Sender).
    /// Sie benachrichtigt die View ("Beobachter"), sobald sich Daten ändern.
    /// Zudem dient sie als Container für die Navigation und hält die User-Session-Daten bereit.
    /// </para>
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // --- Private Felder (Zustandsspeicher) ---
        private Teilnehmer _aktuellerTeilnehmer;

        // Instanzen der untergeordneten ViewModels (Caching-Strategie für statische Seiten)
        private WelcomeViewModel _welcomeViewModel;
        private SettingsViewModel _settingsViewModel;
        private FirmenUebersichtViewModel _firmenUebersichtViewModel;
        private FirmaAnzeigenViewModel _betriebAnzeigenViewModel;

        // Speichert das aktuell sichtbare ViewModel für das ContentControl
        private object _aktuellesViewModel;

        // [NEU] Variable zum Merken der vorherigen Ansicht für die Hilfe-Funktion ("Back-Stack")
        private object _letzteView;

        // --- Öffentliche Eigenschaften (Binding-Quellen) ---

        // Daten für die Anzeige in der Navigationsleiste
        public string TeilnehmerKurs { get; }
        public string TeilnehmerName { get; }

        /// <summary>
        /// Bestimmt, welche Ansicht im ContentControl angezeigt wird.
        /// <para>
        /// HIER GREIFT DAS OBSERVER-PATTERN:
        /// Der Setter ändert den Zustand und löst die Benachrichtigung (OnPropertyChanged) aus.
        /// Die View empfängt diese Nachricht und aktualisiert die Anzeige.
        /// </para>
        /// </summary>
        public object AktuellesViewModel
        {
            get { return _aktuellesViewModel; }
            set
            {
                // 1. Der Zustand ändert sich (Schreibvorgang)
                _aktuellesViewModel = value;

                // 2. Die Benachrichtigung wird gesendet
                // Das "Subjekt" ruft: "Hallo Beobachter (View), mein Wert hat sich geändert!"
                OnPropertyChanged();
            }
        }

        // --- Commands (Steuerung der Buttons) ---
        public ICommand ShowWelcomeViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }
        public ICommand ShowFirmaAnlegenCommand { get; }
        public ICommand ShowFirmenUebersichtCommand { get; }
        public ICommand ShowBetriebAnzeigenCommand { get; }

        // [NEU] Command für den Hilfe-Button
        public ICommand ShowHilfeViewCommand { get; }

        /// <summary>
        /// Konstruktor: Wird beim Programmstart instanziiert.
        /// Lädt Benutzerdaten, initialisiert Sub-ViewModels und Commands.
        /// </summary>
        public MainWindowViewModel()
        {
            // 1. Laden der Sitzungsdaten
            // Zugriff auf das Singleton, um den angemeldeten Benutzer zu ermitteln.
            var sitzung = AktiveSitzung.Instance;
            if (sitzung.IstAngemeldet())
            {
                _aktuellerTeilnehmer = sitzung.AngemeldeterTeilnehmer;
                TeilnehmerKurs = _aktuellerTeilnehmer.Kurs;
                TeilnehmerName = $"{_aktuellerTeilnehmer.Vorname} {_aktuellerTeilnehmer.Nachname}";
            }
            else
            {
                // Fallback-Werte für Design-Mode oder Fehlerfälle
                TeilnehmerKurs = "Nicht angemeldet";
                TeilnehmerName = "Unbekannt";
            }

            // 2. Initialisierung der Unter-ViewModels
            _welcomeViewModel = new WelcomeViewModel();
            _settingsViewModel = new SettingsViewModel();

            // Übergabe der Referenz 'this' (das Hauptfenster), damit Unterseiten
            // Navigationsbefehle an das Hauptfenster senden können (Dependency Injection light).
            _firmenUebersichtViewModel = new FirmenUebersichtViewModel(this);
            _betriebAnzeigenViewModel = new FirmaAnzeigenViewModel();

            // 3. Verknüpfung der Commands mit Methoden
            ShowWelcomeViewCommand = new RelayCommand(ExecuteShowWelcomeView);
            ShowSettingsViewCommand = new RelayCommand(ExecuteShowSettingsView);
            ShowFirmaAnlegenCommand = new RelayCommand(ExecuteShowFirmaAnlegen);
            ShowFirmenUebersichtCommand = new RelayCommand(ExecuteShowFirmenUebersicht);
            ShowBetriebAnzeigenCommand = new RelayCommand(ExecuteShowBetriebAnzeigen);

            // [NEU] Verknüpfung des Hilfe-Commands
            ShowHilfeViewCommand = new RelayCommand(ExecuteShowHilfeView);

            // 4. Festlegen der Startseite
            AktuellesViewModel = _welcomeViewModel;
        }

        // --- Navigations-Methoden (Logik hinter den Commands) ---

        /// <summary>
        /// Zeigt die Willkommens-Seite an.
        /// </summary>
        private void ExecuteShowWelcomeView(object p)
        {
            AktuellesViewModel = _welcomeViewModel;
        }

        /// <summary>
        /// Zeigt die Einstellungs-Seite an.
        /// </summary>
        private void ExecuteShowSettingsView(object p)
        {
            AktuellesViewModel = _settingsViewModel;
        }

        /// <summary>
        /// Zeigt die Firmenübersicht an und aktualisiert die Daten.
        /// </summary>
        private void ExecuteShowFirmenUebersicht(object p)
        {
            // Aktualisierung der Daten beim Wechsel auf die Übersicht erzwingen
            _firmenUebersichtViewModel.ExecuteRefresh(null);
            AktuellesViewModel = _firmenUebersichtViewModel;
        }

        /// <summary>
        /// Zeigt die Betriebs-Anzeige (Platzhalter) an.
        /// </summary>
        private void ExecuteShowBetriebAnzeigen(object p)
        {
            AktuellesViewModel = _betriebAnzeigenViewModel;
        }

        /// <summary>
        /// Navigiert zur Maske "Neue Firma anlegen".
        /// </summary>
        /// <param name="parameter">Wird vom Command übergeben (meist null).</param>
        public void ExecuteShowFirmaAnlegen(object parameter)
        {
            // Erstellung einer neuen Instanz, um leere Eingabefelder zu garantieren.
            // Übergabe von 'this' (für Navigation) und 'null' (weil kein bestehender Datensatz bearbeitet wird).
            AktuellesViewModel = new FirmaAnlegenViewModel(this, null);
        }

        /// <summary>
        /// [NEU] Zeigt die kontextsensitive Hilfe an.
        /// Speichert vorher die aktuelle Ansicht, um ein "Zurück" zu ermöglichen.
        /// </summary>
        private void ExecuteShowHilfeView(object p)
        {
            // 1. Speichern, wo wir gerade sind ("Gedächtnis")
            // Wir müssen prüfen, ob wir nicht schon in der Hilfe sind, sonst überschreiben wir 
            // die echte letzte View mit der Hilfe selbst.
            if (!(AktuellesViewModel is HilfeViewModel))
            {
                _letzteView = AktuellesViewModel;
            }

            // 2. Kontext ermitteln (Woher kommt der User?)
            string kontextKey = "Allgemein";

            // Wir prüfen den Typ des aktuellen ViewModels
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

            // 3. Das Hilfe-ViewModel erstellen
            var hilfeVM = new HilfeViewModel(kontextKey);

            // 4. Die "Zurück"-Funktion injizieren (Dependency Injection)
            // Wir geben dem HilfeViewModel eine Funktion mit, die es ausführen soll, wenn "Schließen" gedrückt wird.
            hilfeVM.SchliessenCommand = new RelayCommand(o =>
            {
                // Status wiederherstellen
                if (_letzteView != null)
                {
                    AktuellesViewModel = _letzteView;
                    _letzteView = null; // Sauber aufräumen
                }
                else
                {
                    // Fallback, falls etwas schief ging
                    AktuellesViewModel = _welcomeViewModel;
                }
            });

            // 5. Ansicht wechseln
            AktuellesViewModel = hilfeVM;
        }

        /// <summary>
        /// Ermöglicht Quernavigation zu einer spezifischen Firmendetail-Seite.
        /// Wird oft von der Firmenübersicht aufgerufen.
        /// </summary>
        /// <param name="firma">Das anzuzeigende Firmen-Objekt.</param>
        public void NavigateToFirmaDetail(Firma firma)
        {
            // Erstellung des Detail-ViewModels mit den konkreten Daten der Firma.
            AktuellesViewModel = new FirmaAnzeigenViewModel(firma);
        }

        // --- INotifyPropertyChanged Implementierung ---

        /// <summary>
        /// Das Event, auf das die View "hört" (Observer-Pattern).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Hilfsmethode zum Auslösen des PropertyChanged-Events.
        /// </summary>
        /// <param name="propertyName">Name der geänderten Eigenschaft (automatisch ermittelt).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}