// Dateipfad: ViewModels/ChangePasswordViewModel.cs

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
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ChangePasswordViewModel()
        {
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void ExecuteSave(object parameter)
        {
            if (parameter is PasswordBox pb)
            {
                string neuesPasswort = pb.Password;
                var currentUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

                if (currentUser == null)
                {
                    MessageBox.Show("Kein Benutzer angemeldet.");
                    return;
                }

                try
                {
                    // Hier prüfen wir das Passwort über den Service
                    bool erfolg = AuthenticationService.Instance.ChangePassword(currentUser.TeilnehmerID, neuesPasswort);

                    if (erfolg)
                    {
                        MessageBox.Show("Passwort erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                        CloseWindow(true);
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "Passwort unsicher", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler: " + ex.Message);
                }
            }
        }

        private void ExecuteCancel(object parameter)
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool result)
        {
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            if (window != null)
            {
                window.DialogResult = result;
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