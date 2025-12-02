using System.ComponentModel;
using System.Runtime.CompilerServices;
using BAT_Man.Services;
using System.Windows.Input;
using System.Windows;
using BAT_Man.Views;

namespace BAT_Man.ViewModels
{
    /// <summary>
    /// Steuert die Einstellungs-Seite.
    /// <para>
    /// FUNKTIONALITÄT:
    /// 1. Umschalten des Designs (Themes) über RadioButtons.
    /// 2. Umschalten der Sprache (Lokalisierung) über RadioButtons.
    /// 3. Öffnen des Passwort-Änderungs-Dialogs.
    /// </para>
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        // --- Private Felder ---
        private string _currentTheme = "dark"; // Standard-Wert beim Start
        private string _currentLanguage = "de"; // Standard-Sprache

        // --- 1. Theme-Eigenschaften (für RadioButtons) ---

        /// <summary>
        /// Steuert den "Dark Mode".
        /// </summary>
        public bool IsDarkMode
        {
            get { return _currentTheme == "dark"; }
            set
            {
                // Logik: Wenn der RadioButton aktiviert wird (value == true)
                if (value)
                {
                    _currentTheme = "dark";
                    // Aufruf des globalen Dienstes zum Laden der XAML-Ressourcen
                    ThemeService.ChangeTheme("dark");
                    NotifyThemeChanged();
                }
            }
        }

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

        /// <summary>
        /// Informiert die GUI, dass sich ALLE Theme-Properties geändert haben könnten.
        /// Das ist notwendig, damit die RadioButtons ihren Status korrekt aktualisieren.
        /// </summary>
        private void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(IsDarkMode));
            OnPropertyChanged(nameof(IsLightMode));
            OnPropertyChanged(nameof(IsBlueMode));
            OnPropertyChanged(nameof(IsGreenMode));
        }

        // --- 2. Sprach-Eigenschaften ---

        /// <summary>
        /// Steuert die Sprache Deutsch.
        /// </summary>
        public bool IsGerman
        {
            get { return _currentLanguage == "de"; }
            set
            {
                if (value)
                {
                    _currentLanguage = "de";
                    // Tauscht das Sprach-Wörterbuch in der App.xaml aus
                    LanguageService.Instance.ChangeLanguage("de");

                    // UI-Update für beide Buttons auslösen
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

        // --- 3. Commands ---

        /// <summary>
        /// Command zum Öffnen des Passwort-Fensters.
        /// </summary>
        public ICommand ChangePasswordCommand { get; }

        // --- Konstruktor ---

        public SettingsViewModel()
        {
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
        }

        // --- Logik ---

        /// <summary>
        /// Öffnet das Fenster 'ChangePasswordWindow' als modalen Dialog.
        /// </summary>
        private void ExecuteChangePassword(object parameter)
        {
            // Neue Instanz des Fensters erstellen (dieses Fenster kennen wir aus Phase 2)
            ChangePasswordWindow passwordWindow = new ChangePasswordWindow();

            // "Owner" setzen: Das Pop-up "gehört" dem Hauptfenster.
            // Verhindert, dass das Pop-up hinter dem Hauptfenster verschwindet.
            if (Application.Current != null)
            {
                passwordWindow.Owner = Application.Current.MainWindow;
            }

            // ShowDialog() pausiert diesen Code, bis das Fenster geschlossen wird.
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