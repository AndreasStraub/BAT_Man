// Dateipfad: ViewModels/AddAktivitaetViewModel.cs
// KORRIGIERTE VERSION (Erklärt die Lambda-Funktion)

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPF_Test.Models;
using WPF_Test.Repositories;
using System.Linq;

namespace WPF_Test.ViewModels
{
    /// <summary>
    /// Das "Gehirn" (ViewModel) für den "Aktivität hinzufügen"-Dialog.
    /// KORREKTUR: Verwaltet jetzt BEIDE Modi ("Neu" und "Bearbeiten")
    /// </summary>
    public class AddAktivitaetViewModel : INotifyPropertyChanged
    {
        // --- 1. Private Felder ---
        private readonly StatusRepository _statusRepository;

        // --- 2. Öffentliche Eigenschaften (für Bindings) ---
        private Aktivitaet _aktivitaetZumBearbeiten;
        public Aktivitaet AktivitaetZumBearbeiten
        {
            get { return _aktivitaetZumBearbeiten; }
            set { _aktivitaetZumBearbeiten = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Status> AlleStatusOptionen { get; set; }

        private Status _ausgewaehlterStatus;
        public Status AusgewaehlterStatus
        {
            get { return _ausgewaehlterStatus; }
            set
            {
                _ausgewaehlterStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditMode { get; private set; }


        // --- 3. Konstruktor (JETZT MIT PARAMETER) ---

        /// <summary>
        /// Konstruktor für den Dialog.
        /// </summary>
        /// <param name="aktivitaet">
        /// Die Aktivität, die bearbeitet werden soll.
        /// Wenn 'null', wird der "Neu"-Modus gestartet.
        /// </param>
        public AddAktivitaetViewModel(Aktivitaet aktivitaet)
        {
            _statusRepository = new StatusRepository();
            AlleStatusOptionen = new ObservableCollection<Status>();
            this.IsEditMode = (aktivitaet != null);

            // 1. Status-Liste laden (wie vorher)
            LadeStatusOptionen();

            // 2. Modus prüfen
            if (aktivitaet == null)
            {
                // --- MODUS: NEU ---
                IsEditMode = false;
                AktivitaetZumBearbeiten = new Aktivitaet
                {
                    Datum = DateTime.Now
                };
            }
            else
            {
                // --- MODUS: BEARBEITEN ---
                IsEditMode = true;

                // Erstelle eine KOPIE (damit "Abbrechen" funktioniert)
                AktivitaetZumBearbeiten = new Aktivitaet
                {
                    Aktivitaet_ID = aktivitaet.Aktivitaet_ID,
                    Datum = aktivitaet.Datum,
                    Kommentar = aktivitaet.Kommentar,
                    Status_ID = aktivitaet.Status_ID,
                    StatusBezeichnung = aktivitaet.StatusBezeichnung
                };

                // WICHTIG: Wähle den korrekten RadioButton
                // (den Status, den die Aktivität bereits hat)

                // ==========================================================
                // HINWEIS: Erklärung der 'FirstOrDefault'-Alternative
                // (Wie von dir gewünscht, ohne Lambda-Ausdruck)
                // ==========================================================

                // 
                // DER "EINFACHE" (lange) WEG, UM DEN STATUS ZU FINDEN:
                //
                // 1. Erstelle eine temporäre Variable, um das Ergebnis zu halten.
                Status tempStatus = null;

                // 2. Durchlaufe jeden einzelnen Status (s) in der vollen Liste
                //    (AlleStatusOptionen).
                foreach (Status s in AlleStatusOptionen)
                {
                    // 3. Prüfe, ob die ID dieses Status (s.Status_ID)
                    //    mit der ID der zu bearbeitenden Aktivität übereinstimmt.
                    if (s.Status_ID == AktivitaetZumBearbeiten.Status_ID)
                    {
                        // 4. TREFFER! Speichere diesen Status...
                        tempStatus = s;

                        // 5. ...und verlasse die Schleife sofort
                        //    (das ist der "First" in 'FirstOrDefault').
                        break;
                    }
                }

                // 6. Weise das Ergebnis (entweder der gefundene Status
                //    oder 'null', falls nichts gefunden wurde) der
                //    eigentlichen Eigenschaft zu.
                AusgewaehlterStatus = tempStatus;

                //
                // DER "MODERNE" (kurze) WEG (Lambda / LINQ):
                // AusgewaehlterStatus = AlleStatusOptionen.FirstOrDefault(
                //     s => s.Status_ID == AktivitaetZumBearbeiten.Status_ID);
                //
                // --- ENDE HINWEIS ---
            }
        }

        // (LadeStatusOptionen bleibt unverändert)
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