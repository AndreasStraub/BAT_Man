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
    /// Das "Gehirn" für das Firma-Formular.
    /// <para>
    /// DRY-PRINZIP (Don't Repeat Yourself):
    /// Dieses ViewModel wird sowohl für das Anlegen (Neu) als auch für das Bearbeiten (Edit) genutzt.
    /// Der Konstruktor entscheidet anhand der Parameter, in welchem Modus wir sind.
    /// </para>
    /// </summary>
    public class FirmaAnlegenViewModel : INotifyPropertyChanged
    {
        // --- Private Felder ---
        private readonly FirmaRepository _firmaRepository;

        // Referenz auf das Hauptfenster (nur im "Neu"-Modus vorhanden, sonst null)
        private readonly MainWindowViewModel _mainVm;

        // --- Öffentliche Eigenschaften ---

        private Firma _firmaZumBearbeiten;
        /// <summary>
        /// Das Datenobjekt, das im Formular angezeigt wird.
        /// </summary>
        public Firma FirmaZumBearbeiten
        {
            get { return _firmaZumBearbeiten; }
            set
            {
                _firmaZumBearbeiten = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gibt an, ob wir eine bestehende Firma bearbeiten (true) oder eine neue anlegen (false).
        /// </summary>
        public bool IsEditMode { get; private set; }

        /// <summary>
        /// Dynamischer Text für den Button (z.B. "Speichern" oder "Aktualisieren").
        /// </summary>
        public string SaveButtonText { get; set; }

        // --- Commands ---
        public ICommand SpeichernCommand { get; }
        public ICommand AbbrechenCommand { get; }

        // --- Konstruktor ---

        /// <summary>
        /// Initialisiert das ViewModel.
        /// </summary>
        /// <param name="mainVm">Referenz zum Hauptfenster (für Navigation bei "Neu"). Kann null sein bei Dialogen.</param>
        /// <param name="firma">Die zu bearbeitende Firma. Wenn null -> Modus "Neu".</param>
        public FirmaAnlegenViewModel(MainWindowViewModel mainVm, Firma firma = null)
        {
            _firmaRepository = new FirmaRepository();
            _mainVm = mainVm;

            SpeichernCommand = new RelayCommand(ExecuteSpeichern);
            AbbrechenCommand = new RelayCommand(ExecuteAbbrechen);

            if (firma == null)
            {
                // --- FALL A: MODUS NEU ---
                // Wir erstellen ein leeres Objekt, damit die TextBoxen nicht abstürzen.
                FirmaZumBearbeiten = new Firma();
                IsEditMode = false;
                SaveButtonText = "Speichern";
            }
            else
            {
                // --- FALL B: MODUS BEARBEITEN ---
                // WICHTIG: Wir erstellen eine KOPIE der Daten!
                // Würden wir das Originalobjekt direkt binden, würden Änderungen in der TextBox
                // sofort in der Liste sichtbar sein, auch wenn man "Abbrechen" klickt.
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

        // --- Logik ---

        /// <summary>
        /// Speichert die Daten in der Datenbank.
        /// </summary>
        /// <param name="parameter">
        /// Optional: Das Fenster (Window), das geschlossen werden soll (im Dialog-Modus).
        /// </summary>
        public void ExecuteSpeichern(object parameter)
        {
            // 1. Validierung (Minimal-Anforderung)
            if (string.IsNullOrEmpty(FirmaZumBearbeiten.Firmenname))
            {
                MessageBox.Show("Der Firmenname darf nicht leer sein.", "Eingabefehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Datenbank-Operation je nach Modus
                if (IsEditMode)
                {
                    _firmaRepository.UpdateFirma(FirmaZumBearbeiten);
                    MessageBox.Show("Firma erfolgreich aktualisiert!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _firmaRepository.AddFirma(FirmaZumBearbeiten);
                    MessageBox.Show("Firma erfolgreich angelegt!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset für nächste Eingabe
                    FirmaZumBearbeiten = new Firma();
                }

                // 3. Dialog schließen (falls wir in einem Fenster sind)
                if (parameter is Window window)
                {
                    window.DialogResult = true; // Signalisiert "Erfolg" an den Aufrufer (FirmaAnzeigenViewModel)
                    window.Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAbbrechen(object parameter)
        {
            if (_mainVm != null)
            {
                // Modus "Neu": Zurück zur Startseite navigieren
                _mainVm.ShowWelcomeViewCommand.Execute(null);
            }
            else if (parameter is Window window)
            {
                // Modus "Bearbeiten": Fenster schließen ohne Speichern
                window.DialogResult = false;
                window.Close();
            }
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}