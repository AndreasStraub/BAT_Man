using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using BAT_Man.Services;
using BAT_Man.Models;

namespace BAT_Man.ViewModels
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
        /// Führt die Validierung und den Speicherprozess aus.
        /// </summary>
        /// <param name="parameter">
        /// Erwartet ein Array (object[]), das zwei PasswordBox-Objekte enthält.
        /// Dies wird durch den MultiValueConverter im XAML ermöglicht.
        /// </param>
        private async void ExecuteSave(object parameter)
        {
            // 1. Parameter-Prüfung und Entpacken des Arrays
            if (parameter is object[] values && values.Length == 2 &&
                values[0] is PasswordBox pbNeu && values[1] is PasswordBox pbBestaetigen)
            {
                // Auslesen der unsicheren Strings aus den PasswordBoxen
                string neuesPasswort = pbNeu.Password;
                string bestaetigung = pbBestaetigen.Password;

                // 2. Vergleich der beiden Eingaben
                if (neuesPasswort != bestaetigung)
                {
                    MessageBox.Show("Die Passwörter stimmen nicht überein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Identifikation des aktuellen Benutzers
                var currentUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

                if (currentUser == null)
                {
                    MessageBox.Show("Kein Benutzer angemeldet.");
                    return;
                }

                // 4. API-Aufruf (asynchron)
                // Der AuthenticationService übernimmt die Kommunikation mit dem Webserver.
                bool erfolg = await AuthenticationService.Instance.ChangePasswordAsync(
                    currentUser.TeilnehmerID,
                    currentUser.RehaNummer,
                    neuesPasswort);

                // 5. Erfolgsbehandlung
                if (erfolg)
                {
                    MessageBox.Show("Passwort erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
                // Im Fehlerfall zeigt der Service bereits eine Meldung an.
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
        /// Hilfsmethode zum Schließen des aktiven Dialogfensters.
        /// </summary>
        /// <param name="result">Das DialogResult, das an den Aufrufer zurückgegeben wird.</param>
        private void CloseWindow(bool result)
        {
            // Ermittlung des aktiven Fensters über die Application-Klasse.
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            if (window != null)
            {
                window.DialogResult = result;
                window.Close();
            }
        }

        #region INotifyPropertyChanged Implementierung

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}