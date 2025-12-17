using System.Windows;
using BAT_Man.ViewModels;
using BAT_Man.Models;

namespace BAT_Man.Views
{
    public partial class AddAktivitaetDialog : Window
    {
        /// <summary>
        /// Status-Flag, das anzeigt, ob der Benutzer den "Löschen"-Button betätigt hat.
        /// <br/>True = Der Datensatz soll entfernt werden.
        /// <br/>False = Der Datensatz soll gespeichert/aktualisiert werden (Standard).
        /// </summary>
        public bool WurdeGeloescht { get; private set; } = false;

        /// <summary>
        /// Konstruktor für den Dialog.
        /// Initialisiert die Komponenten und setzt den DataContext.
        /// </summary>
        /// <param name="aktivitaet">
        /// Das zu bearbeitende Objekt.
        /// 'null' signalisiert den Modus "Neu anlegen".
        /// Ein Objekt signalisiert den Modus "Bearbeiten".
        /// </param>
        public AddAktivitaetDialog(Aktivitaet aktivitaet)
        {
            InitializeComponent();

            // Manuelle Instanziierung des ViewModels und Übergabe der Daten.
            // Dies ist notwendig, da der Konstruktor Parameter erwartet.
            this.DataContext = new AddAktivitaetViewModel(aktivitaet);
        }

        /// <summary>
        /// Event-Handler für den OK-Button.
        /// Führt eine Validierung durch und schließt den Dialog mit Erfolg.
        /// </summary>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // Zugriff auf das ViewModel, um die Benutzereingaben zu prüfen.
            var viewModel = this.DataContext as AddAktivitaetViewModel;

            // Validierung: Es muss zwingend ein Status gewählt sein.
            if (viewModel.AusgewaehlterStatus == null)
            {
                MessageBox.Show("Bitte wählen Sie einen Status aus.", "Eingabefehler");
                return;
            }

            // Setzen des DialogResults schließt das Fenster und gibt "true" an den Aufrufer zurück.
            this.DialogResult = true;
        }

        /// <summary>
        /// Event-Handler für den Löschen-Button.
        /// Fordert eine Bestätigung an und markiert den Vorgang als "Löschen".
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Sicherheitsabfrage, um versehentliches Löschen zu verhindern.
            var result = MessageBox.Show(
                "Möchten Sie diese Aktivität wirklich endgültig löschen?",
                "Löschen bestätigen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Setzen des Flags, damit das aufrufende ViewModel weiß, was zu tun ist.
                this.WurdeGeloescht = true;

                // Schließen des Dialogs mit "true" (Erfolg), damit die Aktion im Hauptfenster ausgeführt wird.
                this.DialogResult = true;
            }
            // Bei "Nein" wird der Vorgang abgebrochen und das Fenster bleibt offen.
        }
    }
}