// Dateipfad: Views/EditFirmaDialogWindow.xaml.cs
// KORRIGIERTE VERSION (Ruft die korrekte Methode auf)

using System.Windows;
using WPF_Test.ViewModels;

namespace WPF_Test.Views
{
    /// <summary>
    /// Interaktionslogik für EditFirmaDialogWindow.xaml
    /// (Der "Container" für Pop-ups)
    /// </summary>
    public partial class EditFirmaDialogWindow : Window
    {
        public EditFirmaDialogWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Benutzer auf "OK" klickt.
        /// </summary>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hole das "Gehirn"
            var viewModel = this.DataContext as FirmaAnlegenViewModel;

            // 2. Sicherheits-Check
            if (viewModel == null)
            {
                MessageBox.Show("Ein interner Fehler ist aufgetreten. ViewModel nicht gefunden.");
                return;
            }

            // 3. Validierung
            if (string.IsNullOrEmpty(viewModel.FirmaZumBearbeiten.Firmenname))
            {
                MessageBox.Show("Der Firmenname darf nicht leer sein.", "Eingabefehler");
                return; // Fenster nicht schließen
            }

            // 4. Dem "Gehirn" sagen, es soll speichern

            // ==========================================================
            // HIER IST DIE KORREKTUR
            // ==========================================================
            // VORHER (FEHLERHAFT):
            // viewModel.Speichern();
            //
            // NACHHER (KORREKT):
            // Wir rufen die Methode auf, die der RelayCommand
            // auch verwendet. Wir übergeben 'null', da der
            // Parameter nicht benutzt wird.
            viewModel.ExecuteSpeichern(null);

            // 5. Erfolg signalisieren und Fenster schließen
            this.DialogResult = true;
            this.Close();
        }
    }
}