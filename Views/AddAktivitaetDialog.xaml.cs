// Dateipfad: Views/AddAktivitaetDialog.xaml.cs

using System.Windows;
using BAT_Man.ViewModels;
// NEU: Wir brauchen das Model
using BAT_Man.Models;

namespace BAT_Man.Views
{
    public partial class AddAktivitaetDialog : Window
    {
        /// <summary>
        /// Speichert, ob der Benutzer "Löschen" geklickt hat.
        /// (true = Löschen, false = OK)
        /// </summary>
        public bool WurdeGeloescht { get; private set; } = false;

        /// <summary>
        /// Konstruktor für den Dialog.
        /// (Erstellt das "Gehirn" (ViewModel) und
        /// übergibt ihm die zu bearbeitende Aktivität)
        /// </summary>
        /// <param name="aktivitaet">
        /// 'null' für "Neu",
        /// ein 'Aktivitaet'-Objekt für "Bearbeiten".
        /// </param>
        public AddAktivitaetDialog(Aktivitaet aktivitaet)
        {
            InitializeComponent();

            // Wir erstellen das "Gehirn" (ViewModel) manuell
            // und übergeben ihm die Aktivität.
            this.DataContext = new AddAktivitaetViewModel(aktivitaet);
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Benutzer auf "OK" klickt.
        /// </summary>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hole das "Gehirn" (ViewModel)
            var viewModel = this.DataContext as AddAktivitaetViewModel;

            // 2. Prüfen, ob ein Status ausgewählt wurde.
            if (viewModel.AusgewaehlterStatus == null)
            {
                MessageBox.Show("Bitte wählen Sie einen Status aus.", "Eingabefehler");
                return;
            }

            // 3. Erfolg signalisieren
            this.DialogResult = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Schritt A: Sicherheitsabfrage
            var result = MessageBox.Show(
                "Möchten Sie diese Aktivität wirklich endgültig löschen?", // TODO: In Sprachdatei auslagern
                "Löschen bestätigen", // TODO: In Sprachdatei auslagern
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Schritt B: Setze unseren "Merkzettel"
                this.WurdeGeloescht = true;

                // Schritt C: Schließe den Dialog mit "Erfolg"
                // (Das Hauptfenster wird bei "true" aufwachen,
                // egal ob wir OK oder Löschen geklickt haben)
                this.DialogResult = true;
            }
            // Wenn "Nein" geklickt wird, passiert nichts.
        }
    }
}