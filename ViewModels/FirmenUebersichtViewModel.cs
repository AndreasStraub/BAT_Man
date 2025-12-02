using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BAT_Man.Models;
using BAT_Man.Repositories;
using BAT_Man.Services;
using BAT_Man.Views;
using BAT_Man.ViewModels;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// ViewModel für die Firmenübersicht.
    /// Verwaltet eine Liste von Firmen und ermöglicht die Navigation zu Details.
    /// </summary>
    public class FirmenUebersichtViewModel : INotifyPropertyChanged
    {
        // --- 1. Private Felder ---

        // Zugriff auf die Datenbank
        private readonly FirmaRepository _firmaRepository;

        // Referenz auf das Hauptfenster (für Navigation)
        private readonly MainWindowViewModel _mainVm;

        // Speichervariable für die ausgewählte Firma
        private Firma _ausgewaehlteFirma;

        // --- 2. Öffentliche Eigenschaften (für Bindings) ---

        /// <summary>
        /// Die Liste der anzuzeigenden Firmen.
        /// WICHTIG: ObservableCollection wird statt List verwendet, 
        /// damit die GUI automatisch merkt, wenn Einträge hinzugefügt oder entfernt werden.
        /// </summary>
        public ObservableCollection<Firma> FirmenListe { get; set; }

        /// <summary>
        /// Das aktuell in der Liste angeklickte Element.
        /// Löst bei Änderung (Setter) das PropertyChanged-Event aus,
        /// damit z.B. Buttons (Bearbeiten) aktiviert/deaktiviert werden können.
        /// </summary>
        public Firma AusgewaehlteFirma
        {
            get { return _ausgewaehlteFirma; }
            set
            {
                _ausgewaehlteFirma = value;
                OnPropertyChanged();

                // Optional: Command-Status aktualisieren ("Darf ich jetzt bearbeiten?")
                // (CommandManager.InvalidateRequerySuggested() geschieht meist automatisch durch WPF)
            }
        }

        // --- 3. Befehle (Commands) für Buttons ---

        /// <summary>Lädt die Liste neu aus der Datenbank.</summary>
        public ICommand RefreshCommand { get; }

        /// <summary>Öffnet die Detailansicht der gewählten Firma.</summary>
        public ICommand BearbeitenCommand { get; }

        // --- 4. Konstruktor ---

        /// <summary>
        /// Erstellt das ViewModel.
        /// </summary>
        /// <param name="mainVm">Wird per Dependency Injection übergeben, um Navigation zu ermöglichen.</param>
        public FirmenUebersichtViewModel(MainWindowViewModel mainVm)
        {
            _firmaRepository = new FirmaRepository();

            // Die Collection muss initialisiert werden, sonst gibt es eine NullReferenceException
            FirmenListe = new ObservableCollection<Firma>();

            _mainVm = mainVm;

            // Commands initialisieren
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            // Bearbeiten ist nur erlaubt, wenn eine Firma ausgewählt ist (CanExecuteBearbeiten)
            BearbeitenCommand = new RelayCommand(ExecuteBearbeiten, CanExecuteBearbeiten);

            // Daten sofort beim Start laden
            ExecuteRefresh(null);
        }

        // --- 5. Ausführungs-Methoden (Logik) ---

        /// <summary>
        /// Lädt alle Firmen aus der Datenbank und füllt die ObservableCollection.
        /// </summary>
        public void ExecuteRefresh(object parameter)
        {
            // Merken der aktuellen Auswahl (falls möglich), um sie nach dem Refresh wiederherzustellen
            var alteAusgewaehlteFirmaId = AusgewaehlteFirma?.Firma_ID;

            // Liste leeren (löst CollectionChanged-Event in der GUI aus -> Liste wird leer)
            FirmenListe.Clear();

            // Daten aus DB holen
            var firmenAusDb = _firmaRepository.GetAlleFirmenMitLetztemStatus();

            // Daten in die ObservableCollection übertragen
            foreach (var firma in firmenAusDb)
            {
                FirmenListe.Add(firma); // Jedes Add aktualisiert die GUI Zeile für Zeile
            }
        }

        /// <summary>
        /// Prüft, ob der "Bearbeiten"-Button klickbar sein darf.
        /// </summary>
        private bool CanExecuteBearbeiten(object parameter)
        {
            // Nur klickbar, wenn tatsächlich eine Zeile ausgewählt ist
            return AusgewaehlteFirma != null;
        }

        /// <summary>
        /// Führt die Navigation zur Detailansicht aus.
        /// </summary>
        private void ExecuteBearbeiten(object parameter)
        {
            // Ruft die Navigationsmethode im Haupt-ViewModel auf
            _mainVm.NavigateToFirmaDetail(AusgewaehlteFirma);
        }

        // --- INotifyPropertyChanged Implementierung ---

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}