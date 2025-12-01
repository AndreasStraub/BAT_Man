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
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _rehaNummer;

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

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            AbbrechenCommand = new RelayCommand(ExecuteAbbrechen);
        }

        // <!*-- ÄNDERUNG: 'async void' ermöglicht das Warten auf den Service --*!>
        private async void ExecuteLogin(object parameter)
        {
            string passwort = "";
            if (parameter is PasswordBox pb)
            {
                passwort = pb.Password;
            }

            // Optional: Hier könnte man einen Ladebalken aktivieren (IsBusy = true)

            // <!*-- ÄNDERUNG: 'await' wartet auf die Antwort vom PHP-Server --*!>
            Teilnehmer user = await AuthenticationService.Instance.Login(RehaNummer, passwort);

            // Optional: Ladebalken deaktivieren (IsBusy = false)

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

        private void ExecuteAbbrechen(object parameter)
        {
            CloseWindow(false);
        }

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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}