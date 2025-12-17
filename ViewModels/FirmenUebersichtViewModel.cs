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

        // Referenz auf das Hauptfenster (erforderlich für die Navigation zu anderen Ansichten)
        private readonly MainWindowViewModel _mainVm;

        // Interner Speicher für das aktuell ausgewählte Listen-Element
        private Firma _ausgewaehlteFirma;

        // --- 2. Öffentliche Eigenschaften (für Bindings) ---

        /// <summary>
        /// Die Liste der anzuzeigenden Firmen.
        /// <para>
        /// ARCHITEKTUR-HINWEIS: Es wird eine 'ObservableCollection' anstelle einer 'List' verwendet.
        /// Dies garantiert, dass die Benutzeroberfläche (das DataGrid) automatisch aktualisiert wird,
        /// sobald Elemente hinzugefügt oder entfernt werden.
        /// </para>
        /// </summary>
        public ObservableCollection<Firma> FirmenListe { get; set; }

        /// <summary>
        /// Das aktuell in der Liste (DataGrid) markierte Element.
        /// </summary>
        public Firma AusgewaehlteFirma
        {
            get { return _ausgewaehlteFirma; }
            set
            {
                _ausgewaehlteFirma = value;
                // Benachrichtigung der UI über die Änderung.
                OnPropertyChanged();

                // Durch das Setzen der Auswahl ändert sich der Rückgabewert von 'CanExecuteBearbeiten'.
                // Das WPF-System prüft daraufhin automatisch, ob Buttons aktiviert werden müssen.
            }
        }

        // --- 3. Befehle (Commands) für Buttons ---

        /// <summary>Lädt die Liste neu aus der Datenbank.</summary>
        public ICommand RefreshCommand { get; }

        /// <summary>Öffnet die Detailansicht der gewählten Firma.</summary>
        public ICommand BearbeitenCommand { get; }

        // --- 4. Konstruktor ---

        /// <summary>
        /// Initialisiert das ViewModel und lädt die Daten.
        /// </summary>
        /// <param name="mainVm">Referenz auf das Haupt-ViewModel (Dependency Injection Light), um Navigationsbefehle abzusetzen.</param>
        public FirmenUebersichtViewModel(MainWindowViewModel mainVm)
        {
            _firmaRepository = new FirmaRepository();

            // Initialisierung der Collection, um NullReferenceExceptions zu vermeiden.
            FirmenListe = new ObservableCollection<Firma>();

            _mainVm = mainVm;

            // Initialisierung der Commands (Verknüpfung von Logik und UI)
            RefreshCommand = new RelayCommand(ExecuteRefresh);

            // Der Bearbeiten-Befehl erhält eine Zusatzbedingung (CanExecute):
            // Er darf nur ausgeführt werden, wenn 'CanExecuteBearbeiten' true zurückgibt.
            BearbeitenCommand = new RelayCommand(ExecuteBearbeiten, CanExecuteBearbeiten);

            // Automatisches Laden der Daten beim Start.
            ExecuteRefresh(null);
        }

        // --- 5. Ausführungs-Methoden (Logik) ---

        /// <summary>
        /// Lädt alle Firmen aus der Datenbank und befüllt die ObservableCollection neu.
        /// </summary>
        /// <param name="parameter">Wird vom Command ignoriert.</param>
        public void ExecuteRefresh(object parameter)
        {
            // Optional: Speichern der ID der aktuell ausgewählten Firma, um die Selektion nach dem Neuladen wiederherzustellen.
            var alteAusgewaehlteFirmaId = AusgewaehlteFirma?.Firma_ID;

            // Leeren der Liste. Dies löst das CollectionChanged-Event aus und leert die Tabelle in der GUI.
            FirmenListe.Clear();

            // Abruf der aktuellen Daten aus der Datenbank (inkl. Status-Informationen).
            var firmenAusDb = _firmaRepository.GetAlleFirmenMitLetztemStatus();

            // Übertragung der Datensätze in die ObservableCollection.
            // Jedes 'Add' löst ein Event aus und fügt eine Zeile im DataGrid hinzu.
            foreach (var firma in firmenAusDb)
            {
                FirmenListe.Add(firma);
            }
        }

        /// <summary>
        /// Prüf-Logik für den Bearbeiten-Button.
        /// </summary>
        /// <param name="parameter">Nicht genutzt.</param>
        /// <returns>Gibt 'true' zurück, wenn eine Firma ausgewählt ist, sonst 'false'.</returns>
        private bool CanExecuteBearbeiten(object parameter)
        {
            // Der Button ist nur aktiv (klickbar), wenn die Auswahl nicht null ist.
            return AusgewaehlteFirma != null;
        }

        /// <summary>
        /// Navigiert zur Detailansicht.
        /// </summary>
        /// <param name="parameter">Nicht genutzt.</param>
        private void ExecuteBearbeiten(object parameter)
        {
            // Delegiert den Navigationswunsch an das Haupt-ViewModel.
            _mainVm.NavigateToFirmaDetail(AusgewaehlteFirma);
        }

        // --- INotifyPropertyChanged Implementierung ---

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Löst das PropertyChanged-Event aus.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}