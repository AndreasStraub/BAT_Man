using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BAT_Man.Models;
using BAT_Man.Repositories;
using System.Linq;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// Steuert den Dialog zum Anlegen oder Bearbeiten einer Aktivität.
    /// Verwaltet den Zustand (Neu vs. Edit) und die Validierung.
    /// </summary>
    public class AddAktivitaetViewModel : INotifyPropertyChanged
    {
        // --- Private Felder ---
        private readonly StatusRepository _statusRepository;

        // --- Öffentliche Eigenschaften ---

        private Aktivitaet _aktivitaetZumBearbeiten;
        /// <summary>
        /// Das Datenobjekt, das im Dialog bearbeitet wird.
        /// </summary>
        public Aktivitaet AktivitaetZumBearbeiten
        {
            get { return _aktivitaetZumBearbeiten; }
            set { _aktivitaetZumBearbeiten = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Liste aller möglichen Status-Werte (für die Auswahl).
        /// </summary>
        public ObservableCollection<Status> AlleStatusOptionen { get; set; }

        private Status _ausgewaehlterStatus;
        /// <summary>
        /// Der aktuell ausgewählte Status.
        /// </summary>
        public Status AusgewaehlterStatus
        {
            get { return _ausgewaehlterStatus; }
            set
            {
                _ausgewaehlterStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gibt an, ob wir eine bestehende Aktivität bearbeiten (true) oder eine neue anlegen (false).
        /// </summary>
        public bool IsEditMode { get; private set; }

        // --- Konstruktor ---

        /// <summary>
        /// Initialisiert das ViewModel.
        /// </summary>
        /// <param name="aktivitaet">
        /// Die Aktivität, die bearbeitet werden soll.
        /// Wenn 'null', wird der Modus "Neu" gestartet.
        /// </param>
        public AddAktivitaetViewModel(Aktivitaet aktivitaet)
        {
            _statusRepository = new StatusRepository();
            AlleStatusOptionen = new ObservableCollection<Status>();

            // Entscheidung: Welcher Modus?
            this.IsEditMode = (aktivitaet != null);

            // 1. Status-Liste laden
            LadeStatusOptionen();

            // 2. Daten vorbereiten
            if (aktivitaet == null)
            {
                // --- MODUS: NEU ---
                AktivitaetZumBearbeiten = new Aktivitaet
                {
                    Datum = DateTime.Now // Standard: Heute
                };

                // Standard-Status auswählen (z.B. der erste in der Liste)
                if (AlleStatusOptionen.Count > 0)
                    AusgewaehlterStatus = AlleStatusOptionen[0];
            }
            else
            {
                // --- MODUS: BEARBEITEN ---
                // WICHTIG: Wir arbeiten auf einer KOPIE, damit "Abbrechen" möglich ist.
                AktivitaetZumBearbeiten = new Aktivitaet
                {
                    Aktivitaet_ID = aktivitaet.Aktivitaet_ID,
                    Datum = aktivitaet.Datum,
                    Kommentar = aktivitaet.Kommentar,
                    Status_ID = aktivitaet.Status_ID,
                    StatusBezeichnung = aktivitaet.StatusBezeichnung
                };

                // Den passenden Status in der Liste finden und auswählen
                SetzeStatusAuswahl(aktivitaet.Status_ID);
            }
        }

        // --- Logik ---

        /// <summary>
        /// Sucht den Status mit der gegebenen ID in der Liste und setzt ihn als ausgewählt.
        /// </summary>
        private void SetzeStatusAuswahl(int gesuchteStatusId)
        {
            // DIDAKTISCHER HINWEIS:
            // Wir nutzen hier eine klassische Schleife statt LINQ (FirstOrDefault),
            // um den Suchvorgang explizit zu zeigen.

            Status gefundenerStatus = null;

            foreach (Status s in AlleStatusOptionen)
            {
                if (s.Status_ID == gesuchteStatusId)
                {
                    gefundenerStatus = s;
                    break; // Gefunden -> Suche beenden
                }
            }

            AusgewaehlterStatus = gefundenerStatus;
        }

        private void LadeStatusOptionen()
        {
            AlleStatusOptionen.Clear();
            var statusAusDb = _statusRepository.GetAlleStatus();
            foreach (var status in statusAusDb)
            {
                AlleStatusOptionen.Add(status);
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
