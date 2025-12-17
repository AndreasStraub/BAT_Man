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
        /// Liste aller möglichen Status-Werte (für die Auswahl in der ListBox).
        /// </summary>
        public ObservableCollection<Status> AlleStatusOptionen { get; set; }

        private Status _ausgewaehlterStatus;
        /// <summary>
        /// Der aktuell vom Benutzer ausgewählte Status.
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
        /// Gibt an, ob eine bestehende Aktivität bearbeitet (true) oder eine neue angelegt wird (false).
        /// Dies steuert u.a. den Fenstertitel und die Sichtbarkeit des Löschen-Buttons.
        /// </summary>
        public bool IsEditMode { get; private set; }

        // --- Konstruktor ---

        /// <summary>
        /// Initialisiert das ViewModel und bereitet die Daten vor.
        /// </summary>
        /// <param name="aktivitaet">
        /// Die Aktivität, die bearbeitet werden soll.
        /// Ist dieser Parameter 'null', wird der Modus "Neu" gestartet.
        /// </param>
        public AddAktivitaetViewModel(Aktivitaet aktivitaet)
        {
            _statusRepository = new StatusRepository();
            AlleStatusOptionen = new ObservableCollection<Status>();

            // Entscheidung über den Betriebsmodus anhand des Parameters.
            this.IsEditMode = (aktivitaet != null);

            // 1. Laden der Stammdaten (Status-Liste)
            LadeStatusOptionen();

            // 2. Vorbereitung des Datenobjekts
            if (aktivitaet == null)
            {
                // --- MODUS: NEU ---
                AktivitaetZumBearbeiten = new Aktivitaet
                {
                    Datum = DateTime.Now // Vorbelegung mit dem aktuellen Datum
                };

                // Standardmäßige Auswahl des ersten Status in der Liste (z.B. "Anruf"), um Leerauswahl zu vermeiden.
                if (AlleStatusOptionen.Count > 0)
                    AusgewaehlterStatus = AlleStatusOptionen[0];
            }
            else
            {
                // --- MODUS: BEARBEITEN ---
                // WICHTIG: Erstellung einer Kopie, um Änderungen verwerfen zu können ("Abbrechen").
                AktivitaetZumBearbeiten = new Aktivitaet
                {
                    Aktivitaet_ID = aktivitaet.Aktivitaet_ID,
                    Datum = aktivitaet.Datum,
                    Kommentar = aktivitaet.Kommentar,
                    Status_ID = aktivitaet.Status_ID,
                    StatusBezeichnung = aktivitaet.StatusBezeichnung
                };

                // Wiederherstellung der Status-Auswahl passend zum Datensatz.
                SetzeStatusAuswahl(aktivitaet.Status_ID);
            }
        }

        // --- Logik ---

        /// <summary>
        /// Sucht den Status mit der gegebenen ID in der Liste und setzt ihn als aktiv.
        /// </summary>
        private void SetzeStatusAuswahl(int gesuchteStatusId)
        {
            Status gefundenerStatus = null;

            // Iteration durch die Liste, um das passende Status-Objekt zu finden.
            foreach (Status s in AlleStatusOptionen)
            {
                if (s.Status_ID == gesuchteStatusId)
                {
                    gefundenerStatus = s;
                    break; // Gefunden -> Abbruch
                }
            }

            AusgewaehlterStatus = gefundenerStatus;
        }

        /// <summary>
        /// Lädt alle verfügbaren Status-Optionen aus der Datenbank.
        /// </summary>
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