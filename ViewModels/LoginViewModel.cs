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
    /// Steuert die Logik des Login-Fensters.
    /// Vermittelt zwischen der View (Eingabe) und dem Model/Service (Datenverarbeitung).
    /// -> (Siehe Dokumentation: 02_Login.md > 2. ViewModel & Commands)
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        // Privates Feld für die Eigenschaft Email.
        // Wurde im vorherigen Code vermisst (Fehler CS0103).
        private string _email;

        // --- Eigenschaften (Data Binding) ---

        /// <summary>
        /// Gebunden an die TextBox für die E-Mail-Adresse.
        /// Löst bei Änderung das PropertyChanged-Event aus, damit die UI synchron bleibt.
        /// </summary>
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        // Hinweis zum Passwort:
        // Passwörter werden aus Sicherheitsgründen nicht als String-Property gebunden (Klartext im Speicher).
        // Stattdessen wird die PasswordBox als Parameter an den Command übergeben.

        // --- Commands (Befehle) ---

        /// <summary>
        /// Befehl zum Ausführen des Login-Vorgangs.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Befehl zum Abbrechen und Schließen der Anwendung.
        /// </summary>
        public ICommand AbbrechenCommand { get; }


        // --- Konstruktor ---

        /// <summary>
        /// Initialisiert das ViewModel und verknüpft die Befehle mit den entsprechenden Methoden.
        /// </summary>
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            AbbrechenCommand = new RelayCommand(ExecuteAbbrechen);
        }


        // --- Logik & Methoden ---

        /// <summary>
        /// Führt die Authentifizierung durch.
        /// </summary>
        /// <param name="parameter">Erwartet die PasswordBox aus der View als Parameter.</param>
        private void ExecuteLogin(object parameter)
        {
            // Extraktion des Passworts aus dem UI-Element (PasswordBox)
            string passwort = "";
            if (parameter is PasswordBox pb)
            {
                passwort = pb.Password;
            }

            // Aufruf des AuthenticationService zur Überprüfung der Daten.
            // Das ViewModel delegiert die "Arbeit" an den Service (Trennung der Zuständigkeiten).
            Teilnehmer user = AuthenticationService.Instance.Login(Email, passwort);

            if (user != null)
            {
                // Fall: Login erfolgreich

                // 1. Benutzer in die globale Sitzung (Singleton) schreiben
                AktiveSitzung.Instance.Anmelden(user);

                // 2. Fenster mit positivem Ergebnis schließen
                CloseWindow(true);
            }
            else
            {
                // Fall: Login fehlgeschlagen
                MessageBox.Show("E-Mail oder Passwort falsch.",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Bricht den Vorgang ab.
        /// </summary>
        private void ExecuteAbbrechen(object parameter)
        {
            // Fenster mit negativem Ergebnis schließen -> führt in App.xaml.cs zum Shutdown.
            CloseWindow(false);
        }

        /// <summary>
        /// Hilfsmethode zum Schließen des zugehörigen Fensters.
        /// Da das ViewModel die View nicht direkt kennt, wird das aktive Fenster gesucht.
        /// </summary>
        /// <param name="dialogResult">Das Ergebnis, das an App.xaml.cs zurückgegeben wird.</param>
        private void CloseWindow(bool dialogResult)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            if (window != null)
            {
                window.DialogResult = dialogResult;
                window.Close();
            }
        }

        // --- INotifyPropertyChanged Implementierung ---

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Informiert die Oberfläche über Änderungen an Eigenschaften.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}