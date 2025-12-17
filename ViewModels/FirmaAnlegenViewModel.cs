using System.ComponentModel;
using System.Runtime.CompilerServices;
using BAT_Man.Models;
using BAT_Man.Repositories;
using System.Windows.Input;
using BAT_Man.Services;
using System.Windows;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// ViewModel für das Firmen-Formular.
    /// <para>
    /// ARCHITEKTUR:
    /// Implementiert das DRY-Prinzip, indem dieselbe Logik für "Neu" und "Bearbeiten" genutzt wird.
    /// Die Unterscheidung erfolgt im Konstruktor anhand der übergebenen Parameter.
    /// </para>
    /// </summary>
    public class FirmaAnlegenViewModel : INotifyPropertyChanged
    {
        // --- Private Felder ---
        private readonly FirmaRepository _firmaRepository;

        // Referenz auf das Hauptfenster (wird nur im Modus "Neu" benötigt, um danach zur Übersicht zu wechseln)
        private readonly MainWindowViewModel _mainVm;

        // --- Öffentliche Eigenschaften ---

        private Firma _firmaZumBearbeiten;

        /// <summary>
        /// Das Datenobjekt, das an die Textfelder der View gebunden ist.
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
        /// Status-Flag zur Unterscheidung der Modi:
        /// <br/>True = Bearbeiten eines bestehenden Datensatzes (Update).
        /// <br/>False = Anlegen eines neuen Datensatzes (Insert).
        /// </summary>
        public bool IsEditMode { get; private set; }

        // --- Commands ---
        public ICommand SpeichernCommand { get; }
        public ICommand AbbrechenCommand { get; }

        // --- Konstruktor ---

        /// <summary>
        /// Initialisiert das ViewModel und bestimmt den Betriebsmodus.
        /// </summary>
        /// <param name="mainVm">Referenz zum Hauptfenster (für Navigation bei "Neu"). Kann bei Dialogen null sein.</param>
        /// <param name="firma">Die zu bearbeitende Firma. Ist dieser Parameter null, wird der Modus "Neu" aktiviert.</param>
        public FirmaAnlegenViewModel(MainWindowViewModel mainVm, Firma firma = null)
        {
            _firmaRepository = new FirmaRepository();
            _mainVm = mainVm;

            SpeichernCommand = new RelayCommand(ExecuteSpeichern);
            AbbrechenCommand = new RelayCommand(ExecuteAbbrechen);

            if (firma == null)
            {
                // --- FALL A: MODUS NEU ---
                // Erstellung eines leeren Objekts zur Initialisierung der Eingabefelder.
                // Verhindert NullReferenceExceptions in der View.
                FirmaZumBearbeiten = new Firma();
                IsEditMode = false;
                //SaveButtonText = "Speichern";
            }
            else
            {
                // --- FALL B: MODUS BEARBEITEN ---
                // WICHTIG: Erstellung einer tiefen Kopie (Deep Copy) der Daten.
                // Würde das Originalobjekt direkt gebunden, wären Änderungen in den Textfeldern
                // sofort in der Hauptliste sichtbar (durch Referenz), selbst wenn der Benutzer "Abbrechen" klickt.
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
                //SaveButtonText = "Aktualisieren";
            }
        }

        // --- Logik ---

        /// <summary>
        /// Führt die Validierung durch und speichert die Daten in der Datenbank.
        /// </summary>
        /// <param name="parameter">
        /// Optional: Das Fenster-Objekt (Window), falls das Formular als Dialog geöffnet wurde.
        /// </param>
        public void ExecuteSpeichern(object parameter)
        {
            // 1. Validierung (Minimal-Anforderung)
            // Prüfung, ob der Firmenname ausgefüllt ist.
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
                    // Aktualisierung eines bestehenden Datensatzes
                    _firmaRepository.UpdateFirma(FirmaZumBearbeiten);
                    MessageBox.Show("Firma erfolgreich aktualisiert!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Einfügen eines neuen Datensatzes
                    _firmaRepository.AddFirma(FirmaZumBearbeiten);
                    MessageBox.Show("Firma erfolgreich angelegt!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset des Objekts für die nächste Eingabe (nur relevant, wenn Fenster offen bleibt)
                    FirmaZumBearbeiten = new Firma();
                }

                // 3. Navigation / Fenster schließen

                // Fallunterscheidung A: Aufruf aus dem Hauptfenster (Modus "Neu")
                if (_mainVm != null)
                {
                    // Navigation zurück zur Übersichtstabelle.
                    _mainVm.ShowFirmenUebersichtCommand.Execute(null);
                }
                // Fallunterscheidung B: Aufruf als Pop-up Dialog (Modus "Bearbeiten")
                else if (parameter is Window window)
                {
                    // Setzen des DialogResults auf true signalisiert dem Aufrufer den Erfolg.
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Bricht den Vorgang ab und navigiert zurück oder schließt das Fenster.
        /// </summary>
        private void ExecuteAbbrechen(object parameter)
        {
            if (_mainVm != null)
            {
                // Modus "Neu": Navigation zurück zur Startseite (WelcomeView).
                _mainVm.ShowWelcomeViewCommand.Execute(null);
            }
            else if (parameter is Window window)
            {
                // Modus "Bearbeiten": Schließen des Fensters mit DialogResult = false (keine Änderung).
                window.DialogResult = false;
                window.Close();
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