// Dateipfad: ViewModels/BetriebAnzeigenViewModel.cs

// ====================================================================
// HINWEIS: WARUM WIR MVVM STATT OnClick VERWENDEN (KISS-Prinzip)
// =Formatiert für Drag-and-Drop am 16.11.2025
// ====================================================================
//
// FRAGE:
// Warum ist dieser Code so "kompliziert"? Warum verwenden wir 
// 'ICommand' (wie 'AddAktivitaetCommand') und 'Binding' 
// (wie 'SelectedItem="{Binding AusgewaehlteAktivitaet}"') 
// statt eines einfachen 'Click="Button_Click"' Events 
// im Code-Behind (*.xaml.cs)?
//
// ANTWORT:
// Das 'OnClick'-Verfahren ist nur für 5 Minuten "simpel".
// Bei einem Projekt wie diesem (Shell, Navigationsleiste, 
// mehrere Seiten, Pop-ups) führt 'OnClick' zu "Spaghetti-Code"
// und einer "Gott-Klasse" im Code-Behind.
//
// Der MVVM-Weg (den wir hier verwenden) ist das 
// WAHRE "KISS"-Prinzip für dieses Projekt, weil er 
// die Verantwortlichkeiten sauber TRENNT (Entkopplung).
//
// --------------------------------------------------------------------
// DER "ALTE" WEG (OnClick im Code-Behind - NICHT KISS)
// --------------------------------------------------------------------
// 1. KOPPLUNG: Das "Gesicht" (View / *.xaml.cs) ist fest mit der
//    Logik (dem "Gehirn") verdrahtet.
// 2. STEUERUNG: Das "Gesicht" (Code-Behind) MUSS das "Gehirn" (ViewModel)
//    kennen und aktiv steuern (z.B. 'viewModel.LadeAktivitaeten();').
// 3. TESTBARKEIT: Fast unmöglich zu testen. Man kann die Doppelklick-Logik
//    nicht testen, ohne das Fenster zu laden und einen echten Klick
//    zu simulieren.
// 4. WARTBARKEIT: Schlecht. Wenn wir den 'AddAktivitaetDialog' ändern,
//    müssen wir auch den Code-Behind dieser Seite (BetriebAnzeigenView) ändern.
//
// --------------------------------------------------------------------
// DER "NEUE" WEG (MVVM - Dieser Code - ECHTES KISS)
// --------------------------------------------------------------------
// 1. ENTKOPPLUNG: Das "Gesicht" (View / *.xaml) ist "dumm".
// 2. SIGNALE: Das "Gesicht" sendet nur Signale (z.B. "Doppelklick passiert!")
//    über 'Command="{Binding EditAktivitaetCommand}"'.
//    Es weiß nicht, WAS als Nächstes passiert.
// 3. LOGIK: Das "Gehirn" (ViewModel / diese C#-Datei) ist "intelligent".
//    Es fängt die Signale ab ('ExecuteEditAktivitaet') und 
//    enthält die GESAMTE Logik (z.B. 'new AddAktivitaetDialog(...)').
// 4. TESTBARKEIT: Perfekt. Wir können diese C#-Datei isoliert testen
//    (Unit-Test), ohne je ein XAML-Fenster zu öffnen.
// 5. WARTBARKEIT: Gut. Das "Gesicht" (XAML) und das "Gehirn" (C#)
//    können unabhängig voneinander geändert werden.
//
// ====================================================================


// Dateipfad: ViewModels/FirmaAnzeigenViewModel.cs
// (Komplett, mit neuem Konstruktor und geänderter Lade-Logik)

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WPF_Test.Models;
using WPF_Test.Repositories;
using WPF_Test.Services;
using WPF_Test.Views;
using System.Windows;
using System.Linq;

namespace WPF_Test.ViewModels
{
    // (Unveränderter Summary...)
    public class FirmaAnzeigenViewModel : INotifyPropertyChanged
    {
        // --- 1. Private Felder ---
        private readonly FirmaRepository _firmaRepository;
        private readonly AktivitaetRepository _aktivitaetRepository;

        // ==========================================================
        // NEUES FELD (Schritt 1 des Plans)
        // ==========================================================
        // Speichert die ID der Firma, die vorausgewählt werden soll
        private readonly int? _firmaIdToPreselect = null;


        // --- 2. Öffentliche Eigenschaften (für Bindings) ---
        public ObservableCollection<Firma> AlleFirmen { get; set; }
            = new ObservableCollection<Firma>();
        public ObservableCollection<Aktivitaet> AktivitaetenListe { get; set; }
            = new ObservableCollection<Aktivitaet>();

        private Firma _ausgewaehlteFirma;
        public Firma AusgewaehlteFirma
        {
            get { return _ausgewaehlteFirma; }
            set
            {
                _ausgewaehlteFirma = value;
                OnPropertyChanged();
                LadeAktivitaeten();
            }
        }

        private Aktivitaet _ausgewaehlteAktivitaet;
        public Aktivitaet AusgewaehlteAktivitaet
        {
            get { return _ausgewaehlteAktivitaet; }
            set { _ausgewaehlteAktivitaet = value; OnPropertyChanged(); }
        }

        // --- 3. Befehle (Commands) ---
        public ICommand FirmaBearbeitenCommand { get; }
        public ICommand AddAktivitaetCommand { get; }
        public ICommand BearbeitenCommand { get; } // (Der umbenannte Befehl)
        public ICommand FirmaLoeschenCommand { get; }

        /// <summary>
        /// Standard-Konstruktor: Wird aufgerufen, wenn die Seite
        /// normal (über die Sidebar) geladen wird.
        /// </summary>
        public FirmaAnzeigenViewModel()
        {
            _firmaRepository = new FirmaRepository();
            _aktivitaetRepository = new AktivitaetRepository();

            LadeAlleFirmenFuerComboBox();

            AddAktivitaetCommand = new RelayCommand(ExecuteAddAktivitaet, CanExecuteAddAktivitaet);
            FirmaBearbeitenCommand = new RelayCommand(ExecuteFirmaBearbeiten, CanExecuteFirmaBearbeiten);
            BearbeitenCommand = new RelayCommand(ExecuteEditAktivitaet, CanExecuteEditAktivitaet);
            FirmaLoeschenCommand = new RelayCommand(ExecuteFirmaLoeschen, CanExecuteFirmaLoeschen);
        }

        // ==========================================================
        // NEUER KONSTRUKTOR (Schritt 1 des Plans)
        // ==========================================================
        /// <summary>
        /// Navigations-Konstruktor: Wird bei Doppelklick
        /// aus der Firmenübersicht aufgerufen.
        /// </summary>
        /// <param name="firmaToSelect">Die Firma, die direkt angezeigt werden soll.</param>
        public FirmaAnzeigenViewModel(Firma firmaToSelect) : this() // Ruft Standard-Konstruktor auf
        {
            // 1. Speichere die ID (das :this() hat bereits die Liste geladen)
            _firmaIdToPreselect = firmaToSelect?.Firma_ID;

            // 2. Wähle die Firma in der ComboBox aus
            if (_firmaIdToPreselect != null)
            {
                // (Die Liste "AlleFirmen" wurde bereits von :this() geladen)
                AusgewaehlteFirma = AlleFirmen.FirstOrDefault(f => f.Firma_ID == _firmaIdToPreselect);
            }
        }


        // --- Lade-Methoden ---

        /// <summary>
        /// Lädt alle Firmen (für die ComboBox) neu aus der DB.
        /// </summary>
        private void LadeAlleFirmenFuerComboBox()
        {
            // Merke dir die aktuell ausgewählte ID
            int? alteId = AusgewaehlteFirma?.Firma_ID;

            AlleFirmen.Clear();
            var firmenAusDb = _firmaRepository.GetAlleFirmen();
            foreach (var firma in firmenAusDb) { AlleFirmen.Add(firma); }

            // ==========================================================
            // GEÄNDERTE LOGIK (Schritt 1 des Plans)
            // ==========================================================
            // Stelle die Auswahl nur wieder her, wenn wir NICHT
            // gerade per Navigation eine Vorauswahl treffen.
            if (_firmaIdToPreselect == null && alteId != null)
            {
                AusgewaehlteFirma = AlleFirmen.FirstOrDefault(f => f.Firma_ID == alteId);
            }
        }

        // (Der Rest der Datei: LadeAktivitaeten, Execute-Methoden, etc.
        // bleibt exakt gleich wie in Ihrer BetriebAnzeigenViewModel.cs)
        private void LadeAktivitaeten()
        {
            AktivitaetenListe.Clear();
            if (AusgewaehlteFirma == null) return;
            var aktivitaetenAusDb =
                _aktivitaetRepository.GetAktivitaetenFuerFirma(AusgewaehlteFirma.Firma_ID);
            foreach (var akt in aktivitaetenAusDb) { AktivitaetenListe.Add(akt); }
        }

        private bool CanExecuteFirmaBearbeiten(object p) { return AusgewaehlteFirma != null; }
        private void ExecuteFirmaBearbeiten(object p)
        {
            // Wir übergeben 'null' als ersten Parameter, da wir im Dialog sind!
            FirmaAnlegenViewModel viewModel = new FirmaAnlegenViewModel(null, AusgewaehlteFirma);

            EditFirmaDialogWindow dialog = new EditFirmaDialogWindow();
            dialog.DataContext = viewModel;
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == true) { LadeAlleFirmenFuerComboBox(); }
        }

        private bool CanExecuteAddAktivitaet(object parameter) { return AusgewaehlteFirma != null; }
        private void ExecuteAddAktivitaet(object parameter)
        {
            AddAktivitaetDialog dialog = new AddAktivitaetDialog(null);
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                var dialogViewModel = dialog.DataContext as AddAktivitaetViewModel;
                Aktivitaet neueAktivitaet = dialogViewModel.AktivitaetZumBearbeiten;
                _aktivitaetRepository.AddAktivitaet(
                    neueAktivitaet,
                    AusgewaehlteFirma.Firma_ID,
                    dialogViewModel.AusgewaehlterStatus.Status_ID
                );
                LadeAktivitaeten();
            }
        }

        private bool CanExecuteEditAktivitaet(object parameter) { return AusgewaehlteAktivitaet != null; }
        private void ExecuteEditAktivitaet(object parameter)
        {
            AddAktivitaetDialog dialog = new AddAktivitaetDialog(AusgewaehlteAktivitaet);
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                if (dialog.WurdeGeloescht)
                {
                    _aktivitaetRepository.DeleteAktivitaet(AusgewaehlteAktivitaet.Aktivitaet_ID);
                }
                else
                {
                    var dialogViewModel = dialog.DataContext as AddAktivitaetViewModel;
                    Aktivitaet bearbeiteteAktivitaet = dialogViewModel.AktivitaetZumBearbeiten;
                    _aktivitaetRepository.UpdateAktivitaet(
                        bearbeiteteAktivitaet,
                        dialogViewModel.AusgewaehlterStatus.Status_ID
                    );
                }
                LadeAktivitaeten();
            }
        }


        // ==========================================================
        // 3. DIE LÖSCH-LOGIK (mit Sicherheitsabfrage)
        // ==========================================================

        private bool CanExecuteFirmaLoeschen(object p)
        {
            // Nur löschbar, wenn eine Firma ausgewählt ist
            return AusgewaehlteFirma != null;
        }

        private void ExecuteFirmaLoeschen(object p)
        {
            // A. Sicherheitsabfrage
            var result = MessageBox.Show(
                $"Möchten Sie die Firma '{AusgewaehlteFirma.Firmenname}' wirklich löschen?\n\nACHTUNG: Alle Aktivitäten und Notizen zu dieser Firma werden ebenfalls unwiderruflich gelöscht!",
                "Firma löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No); // Standardauswahl auf "Nein" zur Sicherheit

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // B. Löschen durchführen (ruft unsere neue Repo-Methode auf)
                    _firmaRepository.DeleteFirma(AusgewaehlteFirma.Firma_ID);

                    MessageBox.Show("Die Firma wurde erfolgreich gelöscht.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    // C. UI aktualisieren
                    // Wir setzen die Auswahl zurück und laden die Liste neu.
                    AusgewaehlteFirma = null;
                    LadeAlleFirmenFuerComboBox();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Löschen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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