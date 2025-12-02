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
        /// Führt die Speicherlogik aus.
        /// </summary>
        /// <param name="parameter">Erwartet ein Array mit zwei PasswordBox-Objekten (durch MultiBinding übergeben).</param>
        // <!*-- ÄNDERUNG: 'async void' ermöglicht das Warten auf den Service (await) --*!>
        private async void ExecuteSave(object parameter)
        {
            // <!*-- NEU: Prüfen, ob der Parameter ein Array von Objekten ist (durch unseren Converter) --*!>
            // Wir erwarten genau zwei PasswordBoxen im Array.
            if (parameter is object[] values && values.Length == 2 &&
                values[0] is PasswordBox pbNeu && values[1] is PasswordBox pbBestaetigen)
            {
                string neuesPasswort = pbNeu.Password;
                string bestaetigung = pbBestaetigen.Password;

                // <!*-- NEU: Vergleich der beiden Passwörter --*!>
                if (neuesPasswort != bestaetigung)
                {
                    MessageBox.Show("Die Passwörter stimmen nicht überein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Abbruch, wenn sie ungleich sind
                }

                var currentUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

                // Sicherheitsprüfung: Ist überhaupt jemand angemeldet?
                if (currentUser == null)
                {
                    MessageBox.Show("Kein Benutzer angemeldet.");
                    return;
                }

                // Aufruf der neuen API-Methode
                // Der Service kümmert sich um Validierung (Länge, Sonderzeichen) und HTTP-Request.
                bool erfolg = await AuthenticationService.Instance.ChangePasswordAsync(
                    currentUser.TeilnehmerID,
                    currentUser.RehaNummer,
                    neuesPasswort);

                if (erfolg)
                {
                    MessageBox.Show("Passwort erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Schließt das Fenster mit positivem Ergebnis
                    CloseWindow(true);
                }
                // Else: Im Fehlerfall hat der Service bereits eine Meldung angezeigt.
            }
            // Optional: Fallback für Single-Parameter (alte Logik), falls benötigt.
            // Ist hier aber nicht mehr nötig, da wir das XAML auf MultiBinding umgestellt haben.
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