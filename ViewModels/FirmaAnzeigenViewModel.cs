using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BAT_Man.Models;
using BAT_Man.Repositories;
using BAT_Man.Services;
using BAT_Man.Views;
using System.Windows;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// Steuert die Detailansicht einer Firma.
    /// <para>
    /// FUNKTIONALITÄT:
    /// 1. Zeigt Details der ausgewählten Firma an.
    /// 2. Lädt und verwaltet die Historie (Aktivitäten) dieser Firma.
    /// 3. Ermöglicht das Bearbeiten, Löschen und Hinzufügen von Daten.
    /// </para>
    /// </summary>
    public class FirmaAnzeigenViewModel : INotifyPropertyChanged
    {
        // --- Private Felder (Interne Datenspeicher und Dienste) ---

        private readonly FirmaRepository _firmaRepository;
        private readonly AktivitaetRepository _aktivitaetRepository;

        // Speichert optional die ID einer Firma, die beim Laden vorselektiert werden soll.
        // Dies ist notwendig für die Navigation per Doppelklick aus der Übersichtstabelle.
        private readonly int? _firmaIdToPreselect = null;

        // --- Öffentliche Eigenschaften (Datenquellen für die View) ---

        /// <summary>
        /// Beinhaltet alle Firmen aus der Datenbank.
        /// Dient als Datenquelle für die ComboBox, um einen schnellen Wechsel zu ermöglichen.
        /// </summary>
        public ObservableCollection<Firma> AlleFirmen { get; set; } = new ObservableCollection<Firma>();

        /// <summary>
        /// Beinhaltet die Aktivitäten (Anrufe, Notizen) der aktuell ausgewählten Firma.
        /// Dient als Datenquelle für die untere ListView (Historie).
        /// </summary>
        public ObservableCollection<Aktivitaet> AktivitaetenListe { get; set; } = new ObservableCollection<Aktivitaet>();

        private Firma _ausgewaehlteFirma;
        /// <summary>
        /// Die aktuell angezeigte Firma.
        /// <para>
        /// LOGIK IM SETTER:
        /// Wird eine neue Firma gesetzt (durch ComboBox oder Navigation),
        /// löst dies automatisch das Nachladen der zugehörigen Aktivitäten aus (LadeAktivitaeten).
        /// </para>
        /// </summary>
        public Firma AusgewaehlteFirma
        {
            get { return _ausgewaehlteFirma; }
            set
            {
                _ausgewaehlteFirma = value;
                OnPropertyChanged();

                // Kaskadierendes Laden: Neue Firma bedeutet neue Aktivitätenliste.
                LadeAktivitaeten();
            }
        }

        private Aktivitaet _ausgewaehlteAktivitaet;
        /// <summary>
        /// Die aktuell in der unteren Liste markierte Aktivität.
        /// Wird benötigt, um bei Klick auf "Bearbeiten" den korrekten Datensatz zu identifizieren.
        /// </summary>
        public Aktivitaet AusgewaehlteAktivitaet
        {
            get { return _ausgewaehlteAktivitaet; }
            set { _ausgewaehlteAktivitaet = value; OnPropertyChanged(); }
        }

        // --- Befehle (Commands) ---

        /// <summary>Öffnet den Dialog zum Bearbeiten der Firmendaten.</summary>
        public ICommand FirmaBearbeitenCommand { get; }

        /// <summary>Löscht die aktuelle Firma nach Sicherheitsabfrage.</summary>
        public ICommand FirmaLoeschenCommand { get; }

        /// <summary>Öffnet den Dialog zum Hinzufügen einer neuen Aktivität.</summary>
        public ICommand AddAktivitaetCommand { get; }

        /// <summary>Öffnet den Dialog zum Bearbeiten einer existierenden Aktivität.</summary>
        public ICommand BearbeitenCommand { get; }

        // --- Konstruktoren ---

        /// <summary>
        /// Standard-Konstruktor: Wird aufgerufen, wenn die Seite über das Menü geöffnet wird.
        /// Initialisiert Repositories und Commands.
        /// </summary>
        public FirmaAnzeigenViewModel()
        {
            _firmaRepository = new FirmaRepository();
            _aktivitaetRepository = new AktivitaetRepository();

            // Initiales Laden aller Daten.
            RefreshDaten();

            // Initialisierung der Commands
            AddAktivitaetCommand = new RelayCommand(ExecuteAddAktivitaet, CanExecuteAddAktivitaet);
            FirmaBearbeitenCommand = new RelayCommand(ExecuteFirmaBearbeiten, CanExecuteFirmaBearbeiten);
            BearbeitenCommand = new RelayCommand(ExecuteEditAktivitaet, CanExecuteEditAktivitaet);
            FirmaLoeschenCommand = new RelayCommand(ExecuteFirmaLoeschen, CanExecuteFirmaLoeschen);
        }

        /// <summary>
        /// Navigations-Konstruktor: Wird aufgerufen, wenn aus der Firmenübersicht navigiert wird.
        /// </summary>
        /// <param name="firmaToSelect">Das Firmen-Objekt, das direkt angezeigt werden soll.</param>
        public FirmaAnzeigenViewModel(Firma firmaToSelect) : this() // Ruft zuerst den Standard-Konstruktor auf (Code-Wiederverwendung)
        {
            // Speichern der ID der gewünschten Firma für die spätere Selektion
            _firmaIdToPreselect = firmaToSelect?.Firma_ID;

            // Suche und Auswahl der Firma in der geladenen Liste
            if (_firmaIdToPreselect != null)
            {
                // Iteration durch die Liste, um das passende Objekt zu finden.
                foreach (Firma f in AlleFirmen)
                {
                    if (f.Firma_ID == _firmaIdToPreselect)
                    {
                        AusgewaehlteFirma = f;
                        break; // Gefunden -> Suche beenden
                    }
                }
            }
        }

        // --- Lade-Logik (Datenbeschaffung) ---

        /// <summary>
        /// Trigger für das Neuladen aller Daten.
        /// </summary>
        public void RefreshDaten()
        {
            LadeAlleFirmenFuerComboBox();
        }

        /// <summary>
        /// Lädt die Liste aller Firmen aus der Datenbank neu.
        /// Versucht dabei, die aktuell ausgewählte Firma wiederherzustellen (Selection Restore),
        /// damit die Auswahl nach einem Refresh nicht verloren geht.
        /// </summary>
        private void LadeAlleFirmenFuerComboBox()
        {
            // 1. Speichern der ID der aktuellen Auswahl
            int? alteId = AusgewaehlteFirma?.Firma_ID;

            // 2. Liste leeren und neu aus der DB füllen
            AlleFirmen.Clear();
            var firmenAusDb = _firmaRepository.GetAlleFirmen();
            foreach (var firma in firmenAusDb) { AlleFirmen.Add(firma); }

            // 3. Auswahl wiederherstellen
            if (alteId != null)
            {
                foreach (Firma f in AlleFirmen)
                {
                    if (f.Firma_ID == alteId)
                    {
                        // Setzen der Eigenschaft löst automatisch 'LadeAktivitaeten()' aus.
                        AusgewaehlteFirma = f;
                        break;
                    }
                }
            }
            // Fallback: Wenn Liste nicht leer ist, aber keine Auswahl besteht -> Wähle das erste Element.
            else if (AlleFirmen.Count > 0)
            {
                AusgewaehlteFirma = AlleFirmen[0];
            }
        }

        /// <summary>
        /// Lädt die Aktivitäten passend zur aktuell ausgewählten Firma.
        /// Wird automatisch aufgerufen, wenn sich 'AusgewaehlteFirma' ändert.
        /// </summary>
        private void LadeAktivitaeten()
        {
            AktivitaetenListe.Clear();

            // Abbruch, wenn keine Firma ausgewählt ist (null).
            if (AusgewaehlteFirma == null) return;

            var aktivitaetenAusDb = _aktivitaetRepository.GetAktivitaetenFuerFirma(AusgewaehlteFirma.Firma_ID);
            foreach (var akt in aktivitaetenAusDb) { AktivitaetenListe.Add(akt); }
        }

        // --- Command-Logik (Interaktion) ---

        /// <summary>
        /// Prüft, ob eine Firma zum Bearbeiten ausgewählt ist.
        /// </summary>
        private bool CanExecuteFirmaBearbeiten(object p) { return AusgewaehlteFirma != null; }

        /// <summary>
        /// Öffnet den Dialog zum Bearbeiten der Firmendaten.
        /// </summary>
        private void ExecuteFirmaBearbeiten(object p)
        {
            // Erstellung des ViewModels für den Dialog (Modus: Bearbeiten)
            FirmaAnlegenViewModel viewModel = new FirmaAnlegenViewModel(null, AusgewaehlteFirma);

            EditFirmaDialogWindow dialog = new EditFirmaDialogWindow();
            dialog.DataContext = viewModel;

            // Wenn der Dialog mit "true" (Speichern) geschlossen wurde -> Liste aktualisieren
            if (dialog.ShowDialog() == true)
            {
                LadeAlleFirmenFuerComboBox();
            }
        }

        /// <summary>
        /// Prüft, ob eine Firma ausgewählt ist, zu der eine Aktivität hinzugefügt werden kann.
        /// </summary>
        private bool CanExecuteAddAktivitaet(object parameter) { return AusgewaehlteFirma != null; }

        /// <summary>
        /// Öffnet den Dialog zum Erstellen einer neuen Aktivität.
        /// </summary>
        private void ExecuteAddAktivitaet(object parameter)
        {
            AddAktivitaetDialog dialog = new AddAktivitaetDialog(null);

            if (dialog.ShowDialog() == true)
            {
                var dialogViewModel = dialog.DataContext as AddAktivitaetViewModel;
                Aktivitaet neueAktivitaet = dialogViewModel.AktivitaetZumBearbeiten;

                // Speichern in der Datenbank
                _aktivitaetRepository.AddAktivitaet(
                    neueAktivitaet,
                    AusgewaehlteFirma.Firma_ID,
                    dialogViewModel.AusgewaehlterStatus.Status_ID
                );

                // Ansicht aktualisieren
                LadeAktivitaeten();
            }
        }

        /// <summary>
        /// Prüft, ob eine Aktivität in der unteren Liste ausgewählt wurde.
        /// </summary>
        private bool CanExecuteEditAktivitaet(object parameter) { return AusgewaehlteAktivitaet != null; }

        /// <summary>
        /// Öffnet den Dialog zum Bearbeiten oder Löschen einer Aktivität.
        /// </summary>
        private void ExecuteEditAktivitaet(object parameter)
        {
            AddAktivitaetDialog dialog = new AddAktivitaetDialog(AusgewaehlteAktivitaet);

            if (dialog.ShowDialog() == true)
            {
                // Fallunterscheidung: Wurde gelöscht oder gespeichert?
                if (dialog.WurdeGeloescht)
                {
                    _aktivitaetRepository.DeleteAktivitaet(AusgewaehlteAktivitaet.Aktivitaet_ID);
                }
                else
                {
                    var dialogViewModel = dialog.DataContext as AddAktivitaetViewModel;
                    _aktivitaetRepository.UpdateAktivitaet(
                        dialogViewModel.AktivitaetZumBearbeiten,
                        dialogViewModel.AusgewaehlterStatus.Status_ID
                    );
                }
                // Ansicht aktualisieren
                LadeAktivitaeten();
            }
        }

        /// <summary>
        /// Prüft, ob eine Firma zum Löschen ausgewählt ist.
        /// </summary>
        private bool CanExecuteFirmaLoeschen(object p) { return AusgewaehlteFirma != null; }

        /// <summary>
        /// Führt den Löschvorgang mit vorheriger Sicherheitsabfrage durch.
        /// </summary>
        private void ExecuteFirmaLoeschen(object p)
        {
            var result = MessageBox.Show(
                $"Möchten Sie die Firma '{AusgewaehlteFirma.Firmenname}' wirklich löschen?\n\nACHTUNG: Alle Aktivitäten und Notizen werden ebenfalls unwiderruflich gelöscht!",
                "Firma löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Löschen der Firma (inkl. kaskadierendem Löschen der Aktivitäten in der DB-Logik)
                    _firmaRepository.DeleteFirma(AusgewaehlteFirma.Firma_ID);
                    MessageBox.Show("Die Firma wurde erfolgreich gelöscht.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Auswahl zurücksetzen und Liste neu laden
                    AusgewaehlteFirma = null;
                    LadeAlleFirmenFuerComboBox();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Fehler beim Löschen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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