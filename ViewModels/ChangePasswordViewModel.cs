using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using WPF_Test.Services;
using WPF_Test.Models;

namespace WPF_Test.ViewModels
{
    /// <summary>
    /// Steuert die Logik für den Passwort-Änderungs-Dialog.
    /// Implementiert INotifyPropertyChanged für potenzielle UI-Updates.
    /// </summary>
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        // Commands für die Interaktion mit der View
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Konstruktor: Initialisiert die Commands.
        /// </summary>
        public ChangePasswordViewModel()
        {
            // Verknüpft die Commands mit den entsprechenden Methoden
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        /// <summary>
        /// Führt die Speicherlogik aus.
        /// </summary>
        /// <param name="parameter">Erwartet das PasswordBox-Objekt aus der View.</param>
        private void ExecuteSave(object parameter)
        {
            // 1. Parameter-Prüfung: Ist der Parameter wirklich eine PasswordBox?
            if (parameter is PasswordBox pb)
            {
                string neuesPasswort = pb.Password;
                var currentUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

                // Sicherheitsprüfung: Ist überhaupt jemand angemeldet?
                if (currentUser == null)
                {
                    MessageBox.Show("Kein Benutzer angemeldet.");
                    return;
                }

                try
                {
                    // 2. Aufruf des Business-Services (Validierung und Speicherung)
                    bool erfolg = AuthenticationService.Instance.ChangePassword(currentUser.TeilnehmerID, neuesPasswort);

                    if (erfolg)
                    {
                        MessageBox.Show("Passwort erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Schließt das Fenster mit positivem Ergebnis
                        CloseWindow(true);
                    }
                }
                catch (ArgumentException ex)
                {
                    // Fängt Validierungsfehler ab (z.B. "Passwort zu kurz")
                    MessageBox.Show(ex.Message, "Passwort unsicher", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    // Fängt allgemeine Fehler ab (z.B. Datenbank nicht erreichbar)
                    MessageBox.Show("Fehler: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Bricht den Vorgang ab und schließt das Fenster.
        /// </summary>
        private void ExecuteCancel(object parameter)
        {
            CloseWindow(false);
        }

        /// <summary>
        /// Hilfsmethode zum Schließen des aktiven Fensters.
        /// </summary>
        /// <param name="result">Das DialogResult (true = Erfolg, false = Abbruch).</param>
        private void CloseWindow(bool result)
        {
            // Sucht das aktuell aktive Fenster der Anwendung
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            if (window != null)
            {
                window.DialogResult = result; // Setzt den Rückgabewert für ShowDialog() in App.xaml.cs
                window.Close();
            }
        }

        #region INotifyPropertyChanged Implementierung

        // Das Event, auf das die View "hört"
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Löst das PropertyChanged-Event aus, um die GUI zu aktualisieren.
        /// </summary>
        /// <param name="propertyName">Name der geänderten Eigenschaft (automatisch ermittelt).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}