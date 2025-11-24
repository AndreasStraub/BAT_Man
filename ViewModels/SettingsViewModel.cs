// Dateipfad: ViewModels/SettingsViewModel.cs
// KORRIGIERTE VERSION (Themes + Sprachwahl + Passwort-Dialog)

using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPF_Test.Services;
// ==========================================================
// NEUE USINGS (Wichtig für Commands und Fenster)
// ==========================================================
using System.Windows.Input; // Für ICommand
using System.Windows;       // Für Application.Current
using WPF_Test.Views;       // Damit wir 'ChangePasswordWindow' kennen

namespace WPF_Test.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        // --- 1. Eigenschaften für die Bindung ---

        private string _currentTheme = "dark"; // Standard ist Dark

        // Property für RadioButton 1
        public bool IsDarkMode
        {
            get { return _currentTheme == "dark"; }
            set
            {
                if (value)
                {
                    _currentTheme = "dark";
                    ThemeService.ChangeTheme("dark");
                    NotifyThemeChanged();
                }
            }
        }

        // Property für RadioButton 2
        public bool IsLightMode
        {
            get { return _currentTheme == "light"; }
            set
            {
                if (value)
                {
                    _currentTheme = "light";
                    ThemeService.ChangeTheme("light");
                    NotifyThemeChanged();
                }
            }
        }

        // Property für RadioButton 3
        public bool IsBlueMode
        {
            get { return _currentTheme == "blue"; }
            set
            {
                if (value)
                {
                    _currentTheme = "blue";
                    ThemeService.ChangeTheme("blue");
                    NotifyThemeChanged();
                }
            }
        }

        // Property für RadioButton 4
        public bool IsGreenMode
        {
            get { return _currentTheme == "green"; }
            set
            {
                if (value)
                {
                    _currentTheme = "green";
                    ThemeService.ChangeTheme("green");
                    NotifyThemeChanged();
                }
            }
        }

        // Hilfsmethode, um Code-Duplizierung zu vermeiden (Optional, aber sauberer)
        private void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(IsDarkMode));
            OnPropertyChanged(nameof(IsLightMode));
            OnPropertyChanged(nameof(IsBlueMode));
            OnPropertyChanged(nameof(IsGreenMode));
        }

        // --- Sprach-Eigenschaften ---
        private string _currentLanguage = "de"; // Standard ist Deutsch
        public bool IsGerman
        {
            get { return _currentLanguage == "de"; }
            set
            {
                if (value)
                {
                    _currentLanguage = "de";
                    LanguageService.Instance.ChangeLanguage("de");
                    OnPropertyChanged(nameof(IsGerman));
                    OnPropertyChanged(nameof(IsEnglish));
                }
            }
        }

        public bool IsEnglish
        {
            get { return _currentLanguage == "en"; }
            set
            {
                if (value)
                {
                    _currentLanguage = "en";
                    LanguageService.Instance.ChangeLanguage("en");
                    OnPropertyChanged(nameof(IsGerman));
                    OnPropertyChanged(nameof(IsEnglish));
                }
            }
        }

        // ==========================================================
        // 2. NEUER COMMAND (Für den Button)
        // ==========================================================
        public ICommand ChangePasswordCommand { get; }


        // --- Konstruktor ---
        public SettingsViewModel()
        {
            // ==========================================================
            // 3. COMMAND INITIALISIEREN
            // ==========================================================
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
        }


        // ==========================================================
        // 4. DIE LOGIK ZUM ÖFFNEN DES FENSTERS
        // ==========================================================
        private void ExecuteChangePassword(object parameter)
        {
            // A. Wir erstellen eine neue Instanz des Passwort-Fensters
            ChangePasswordWindow passwordWindow = new ChangePasswordWindow();

            // B. Wir setzen das Hauptfenster als "Besitzer".
            //    Das sorgt dafür, dass das Pop-up immer VOR dem Hauptfenster bleibt.
            if (Application.Current != null)
            {
                passwordWindow.Owner = Application.Current.MainWindow;
            }

            // C. Wir öffnen das Fenster "modal".
            //    Der Benutzer muss es erst schließen, bevor er zurück kann.
            passwordWindow.ShowDialog();
        }


        // --- INotifyPropertyChanged Implementierung ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}