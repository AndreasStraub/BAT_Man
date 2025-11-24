// Dateipfad: ViewModels/FirmaAnlegenViewModel.cs
// KORRIGIERTE VERSION (Methodenname korrigiert)

using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPF_Test.Models;
using WPF_Test.Repositories;
using System.Windows.Input;
using WPF_Test.Services;
using System.Windows;

namespace WPF_Test.ViewModels
{
    /// <summary>
    /// Das "Gehirn" (ViewModel) für das Firma-Formular.
    /// Es verwaltet BEIDE Modi: "Neu" und "Bearbeiten".
    /// </summary>
    public class FirmaAnlegenViewModel : INotifyPropertyChanged
    {
        // --- 1. Private Felder ---
        private readonly FirmaRepository _firmaRepository;
        private readonly MainWindowViewModel _mainVm;

        // --- 2. Öffentliche Eigenschaften (für Bindings) ---
        private Firma _firmaZumBearbeiten;
        public Firma FirmaZumBearbeiten
        {
            get { return _firmaZumBearbeiten; }
            set
            {
                _firmaZumBearbeiten = value;
                OnPropertyChanged();
            }
        }
        public bool IsEditMode { get; private set; }
        public string SaveButtonText { get; set; }

        // --- 3. Befehle (Commands) ---
        public ICommand SpeichernCommand { get; }

        public ICommand AbbrechenCommand { get; }

        // --- 4. Konstruktor ---
        // Wir fügen 'MainWindowViewModel mainVm' als ersten Parameter hinzu.
        // 'firma' bleibt optional (null = Neu, Objekt = Bearbeiten)
        public FirmaAnlegenViewModel(MainWindowViewModel mainVm, Firma firma = null)
        {
            _firmaRepository = new FirmaRepository();
            _mainVm = mainVm; // Speichern für später

            // DIESE ZEILE ist der Grund, warum der Methodenname
            // 'ExecuteSpeichern' lauten muss.
            SpeichernCommand = new RelayCommand(ExecuteSpeichern);

            AbbrechenCommand = new RelayCommand(ExecuteAbbrechen);

            if (firma == null)
            {
                // --- MODUS: NEU ---
                FirmaZumBearbeiten = new Firma();
                IsEditMode = false;
                SaveButtonText = "Speichern"; // (Holt Text aus Sprachdatei)
            }
            else
            {
                // --- MODUS: BEARBEITEN ---
                FirmaZumBearbeiten = new Firma
                {
                    Firma_ID = firma.Firma_ID,
                    Teilnehmer_ID = firma.Teilnehmer_ID,
                    Firmenname = firma.Firmenname,
                    Strasse = firma.Strasse,
                    Hausnummer = firma.Hausnummer,
                    PLZ = firma.PLZ,
                    Ort = firma.Ort,
                    Ansprechpartner = firma.Ansprechpartner,
                    Telefon = firma.Telefon,
                    EMail = firma.EMail
                };
                IsEditMode = true;
                SaveButtonText = "Aktualisieren";
            }
        }

        // ==========================================================
        // HIER IST DIE KORREKTUR
        // ==========================================================

        /// <summary>
        /// Die Haupt-Logik (wird von "OK" oder "Speichern" aufgerufen).
        /// </summary>
        // VORHER (FEHLERHAFT):
        // public void Speichern()
        //
        // NACHHER (KORREKT):
        // Die Methode muss 'ExecuteSpeichern' heißen und '(object parameter)'
        // annehmen, damit der 'RelayCommand' sie findet.
        public void ExecuteSpeichern(object parameter)
        {
            // Validierung
            if (string.IsNullOrEmpty(FirmaZumBearbeiten.Firmenname))
            {
                MessageBox.Show("Der Firmenname darf nicht leer sein.", "Eingabefehler");
                return; // Abbruch
            }

            if (IsEditMode)
            {
                _firmaRepository.UpdateFirma(FirmaZumBearbeiten);
                MessageBox.Show("Firma erfolgreich aktualisiert!");
            }
            else
            {
                _firmaRepository.AddFirma(FirmaZumBearbeiten);
                MessageBox.Show("Firma erfolgreich gespeichert!");

                // Formular leeren (nur im "Neu"-Modus)
                FirmaZumBearbeiten = new Firma();
            }
        }

        private void ExecuteAbbrechen(object parameter)
        {
            // Wenn wir eine Referenz zum Hauptfenster haben (also im "Neu"-Modus sind),
            // navigieren wir zurück zur Willkommensseite.
            if (_mainVm != null)
            {
                // Wir rufen einfach den Befehl des Hauptfensters auf
                _mainVm.ShowWelcomeViewCommand.Execute(null);
            }
            else
            {
                // Falls wir im Dialog-Modus sind (Bearbeiten), könnten wir hier
                // das Fenster schließen. Aber meistens hat der Dialog eigene Buttons.
                // Für jetzt reicht es, nichts zu tun oder eine Meldung.
            }
        }

        // --- INotifyPropertyChanged Implementierung ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}