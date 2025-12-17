using System.Windows;
using BAT_Man.Views;
using BAT_Man.ViewModels;
using BAT_Man.Services;

namespace BAT_Man
{
    /// <summary>
    /// Interaktionslogik für "App.xaml".
    /// Dient als "Bootstrapper" der Anwendung.
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
            // Basis-Initialisierung von WPF aufrufen (lädt App.xaml)
            base.OnStartup(e);

            // ------------------------------------------------------------
            // PHASE 1: Authentifizierung (Login)
            // ------------------------------------------------------------

            // "Wiring": Manuelle Verbindung von View und ViewModel, 
            // da hier noch keine DataTemplates greifen (das Fenster wird manuell geöffnet).
            LoginViewModel loginVm = new LoginViewModel();
            LoginWindow loginWindow = new LoginWindow();

            // "Wiring": Manuelle Verbindung von View und ViewModel, 
            // da hier noch keine DataTemplates greifen, wird das Fenster manuell geöffnet.
            // (Verbindung View <-> ViewModel)
            loginWindow.DataContext = loginVm;

            // Modale Anzeige des Fensters.
            // Der Code hält in der nächsten Zeile an, bis der User klickt.
            bool? loginResult = loginWindow.ShowDialog();

            // Auswertung: Hat der User erfolgreich "Anmelden" geklickt?
            if (loginResult != true)
            {
                // Falls "Abbrechen" oder Fenster geschlossen -> App komplett beenden.
                Shutdown();
                return;
            }

            // ------------------------------------------------------------
            // PHASE 2: Sicherheitsprüfung (Passwort-Zwang)
            // ------------------------------------------------------------

            // Zugriff auf den globalen Singleton-Status des angemeldeten Benutzers
            var aktuellerUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

            // Prüfen, ob das Flag "MussPasswortAendern" in der DB gesetzt war
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

                // Erneutes modales Warten
                bool? pwResult = pwWindow.ShowDialog();

                if (pwResult != true)
                {
                    // Wer das Passwort nicht ändert, darf nicht rein.
                    Shutdown();
                    return;
                }
            }

            // ------------------------------------------------------------
            // PHASE 3: Initialisierung Hauptfenster
            // ------------------------------------------------------------

            // Wenn das Programm hier ankommt, ist der User authentifiziert.
            MainWindowViewModel mainVm = new MainWindowViewModel();
            MainWindow mainWindow = new MainWindow();

            // Verbindung herstellen
            mainWindow.DataContext = mainVm;

            // Dieses Fenster als das "Hauptfenster" der App registrieren
            this.MainWindow = mainWindow;

            // Fenster anzeigen (nicht modal, der Code läuft weiter)
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
