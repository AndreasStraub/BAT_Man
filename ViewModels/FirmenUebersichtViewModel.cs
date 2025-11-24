// Dateipfad: ViewModels/FirmenUebersichtViewModel.cs
// (Komplett, mit KORRIGIERTEM 'AusgewaehlteFirma' Getter/Setter)

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WPF_Test.Models;
using WPF_Test.Repositories;
using WPF_Test.Services;
using WPF_Test.Views;
using WPF_Test.ViewModels; // (Das 'using' von Schritt 2)


namespace WPF_Test.ViewModels
{
    public class FirmenUebersichtViewModel : INotifyPropertyChanged
    {
        // --- 1. Private Felder ---
        private readonly FirmaRepository _firmaRepository;
        private readonly MainWindowViewModel _mainVm; // (Das Feld von Schritt 2)


        // --- 2. Öffentliche Eigenschaften (für Bindings) ---

        public ObservableCollection<Firma> FirmenListe { get; set; }


        // ==========================================================
        // HIER IST DIE KORREKTUR
        // (Der Platzhalter wurde durch die volle Implementierung ersetzt)
        // ==========================================================
        private Firma _ausgewaehlteFirma;
        public Firma AusgewaehlteFirma
        {
            get { return _ausgewaehlteFirma; }
            set
            {
                _ausgewaehlteFirma = value;
                OnPropertyChanged();
            }
        }
        // ==========================================================


        // --- 3. Befehle (Commands) für Buttons ---
        public ICommand RefreshCommand { get; }
        public ICommand BearbeitenCommand { get; }


        // --- 4. Konstruktor ---
        // (Dieser ist der neue Konstruktor von Schritt 2)
        public FirmenUebersichtViewModel(MainWindowViewModel mainVm)
        {
            _firmaRepository = new FirmaRepository();
            FirmenListe = new ObservableCollection<Firma>();

            _mainVm = mainVm;

            RefreshCommand = new RelayCommand(ExecuteRefresh);
            BearbeitenCommand = new RelayCommand(ExecuteBearbeiten, CanExecuteBearbeiten);

            ExecuteRefresh(null);
        }


        // --- 5. Ausführungs-Methoden (Die Logik) ---

        public void ExecuteRefresh(object parameter)
        {
            // Diese Zeile WIRD JETZT KOMPILIEREN, da
            // 'AusgewaehlteFirma' wieder einen 'get'-Accessor hat.
            var alteAusgewaehlteFirmaId = AusgewaehlteFirma?.Firma_ID;

            FirmenListe.Clear();
            var firmenAusDb = _firmaRepository.GetAlleFirmenMitLetztemStatus();
            foreach (var firma in firmenAusDb)
            {
                FirmenListe.Add(firma);
            }
        }

        private bool CanExecuteBearbeiten(object parameter)
        {
            // Diese Zeile WIRD JETZT KOMPILIEREN.
            return AusgewaehlteFirma != null;
        }

        // (Dies ist die neue Navigations-Logik von Schritt 2)
        private void ExecuteBearbeiten(object parameter)
        {
            // Diese Zeile WIRD JETZT KOMPILIEREN.
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