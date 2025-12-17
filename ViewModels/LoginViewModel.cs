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
    /// ViewModel für das Login-Fenster.
    /// Verwaltet die Eingaben (Reha-Nummer, Passwort) und steuert den Authentifizierungsprozess.
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _rehaNummer;

        /// <summary>
        /// Die vom Benutzer eingegebene Reha-Nummer.
        /// Durch das Two-Way-Binding in der View landet die Eingabe automatisch hier.
        /// </summary>
        public string RehaNummer
        {
            get { return _rehaNummer; }
            set
            {
                _rehaNummer = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand AbbrechenCommand { get; }

        /// <summary>
        /// Konstruktor: Initialisiert die Befehle (Commands).
        /// </summary>
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            AbbrechenCommand = new RelayCommand(ExecuteAbbrechen);
        }

        /// <summary>
        /// Führt den Login-Vorgang aus.
        /// </summary>
        /// <param name="parameter">
        /// Erwartet das PasswordBox-Control aus der View.
        /// Dies ist notwendig, da das Passwort-Feld aus Sicherheitsgründen kein direktes Binding unterstützt.
        /// </param>
        private async void ExecuteLogin(object parameter)
        {
            string passwort = "";
            if (parameter is PasswordBox pb)
            {
                passwort = pb.Password;
            }

            Teilnehmer user = await AuthenticationService.Instance.Login(RehaNummer, passwort);
            if (user != null)
            {
                AktiveSitzung.Instance.Anmelden(user);
                CloseWindow(true);
            }
            else
            {
                MessageBox.Show("Reha-Nummer oder Passwort falsch (oder Server nicht erreichbar).",
                                "Login fehlgeschlagen",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Bricht den Login ab und schließt die Anwendung.
        /// </summary>
        /// <param name="parameter">Nicht verwendet.</param>
        private void ExecuteAbbrechen(object parameter)
        {
            CloseWindow(false);
        }

        /// <summary>
        /// Hilfsmethode zum Schließen des aktiven Fensters.
        /// Setzt das DialogResult, um dem Aufrufer (App.xaml.cs) den Status mitzuteilen.
        /// </summary>
        /// <param name="dialogResult">True = Erfolg, False = Abbruch.</param>
        private void CloseWindow(bool dialogResult)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            if (window != null)
            {
                window.DialogResult = dialogResult;
                window.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Feuert das Event, wenn sich eine Eigenschaft ändert.
        /// </summary>
        /// <param name="propertyName">Name der geänderten Eigenschaft (automatisch ermittelt).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}