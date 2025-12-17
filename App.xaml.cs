using System.Windows;
using BAT_Man.Views;
using BAT_Man.ViewModels;
using BAT_Man.Services;

namespace BAT_Man
{
    /// <summary>
    /// Interaktionslogik für "App.xaml".
    /// Steuert den manuellen Programmstart, die Authentifizierung und die Initialisierung des Hauptfensters.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Einstiegspunkt der Anwendung.
        /// Ersetzt die Standard-StartupUri und implementiert die Login-Kette. 
        /// </summary>
        /// <param name="e">Startargumente der Anwendung.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Instanziierung von ViewModel und View
            LoginViewModel loginVm = new LoginViewModel();
            LoginWindow loginWindow = new LoginWindow();

            // Manuelle Zuweisung des DataContext (Verbindung View <-> ViewModel)
            loginWindow.DataContext = loginVm;

            // Modale Anzeige des Fensters.
            bool? loginResult = loginWindow.ShowDialog();

            // Auswertung des Login-Ergebnisses
            if (loginResult != true)
            {
                Shutdown();
                return;
            }

            // Zugriff auf den globalen Singleton-Status des angemeldeten Benutzers
            var aktuellerUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

            if (aktuellerUser != null && aktuellerUser.MussPasswortAendern)
            {
                // Diese Meldung ist jetzt im ChangePasswordWindow integriert.
                //MessageBox.Show("Sie müssen Ihr Passwort ändern, bevor Sie fortfahren können.",
                //                "Sicherheitshinweis",
                //                MessageBoxButton.OK,
                //                MessageBoxImage.Information);

                ChangePasswordViewModel pwVm = new ChangePasswordViewModel();

                ChangePasswordWindow pwWindow = new ChangePasswordWindow();

                pwWindow.DataContext = pwVm;

                bool? pwResult = pwWindow.ShowDialog();

                if (pwResult != true)
                {
                    Shutdown();
                    return;
                }
            }

            MainWindowViewModel mainVm = new MainWindowViewModel();
            MainWindow mainWindow = new MainWindow();

            mainWindow.DataContext = mainVm;
  
            this.MainWindow = mainWindow;
            mainWindow.Show();

            // ============================================================
            // Der Modus wird jetzt geändert:    
            // Jetzt, wo das Hauptfenster offen ist, wird die Regel geändert.
            // Ab jetzt gilt: "Wenn das Hauptfenster zugeht, beende dich."
            // ============================================================
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}