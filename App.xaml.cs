using System.Windows;
using WPF_Test.Views;
using WPF_Test.ViewModels;
using WPF_Test.Services;

namespace WPF_Test
{
    /// <summary>
    /// Interaktionslogik für "App.xaml".
    /// Steuert den manuellen Programmstart, die Authentifizierung und die Initialisierung des Hauptfensters.
    /// -> (Siehe Dokumentation: 01_Start_App.md)
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

            // ------------------------------------------------------------
            // PHASE 1: Authentifizierung (Login)
            // -> (Siehe Dokumentation: 01_Start_App.md > Programmablauf Phase 1)
            // ------------------------------------------------------------

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

            // ------------------------------------------------------------
            // PHASE 2: Sicherheitsprüfung (Passwort-Zwang)
            // -> (Siehe Dokumentation: 01_Start_App.md > Programmablauf Phase 2)
            // ------------------------------------------------------------

            // Zugriff auf den globalen Singleton-Status des angemeldeten Benutzers
            var aktuellerUser = AktiveSitzung.Instance.AngemeldeterTeilnehmer;

            if (aktuellerUser != null && aktuellerUser.MussPasswortAendern)
            {
                MessageBox.Show("Sie müssen Ihr Passwort ändern, bevor Sie fortfahren können.",
                                "Sicherheitshinweis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                // KORREKTUR: Wir nutzen den Standard-Konstruktor, passend zu deinem aktuellen Code-Stand.
                ChangePasswordViewModel pwVm = new ChangePasswordViewModel();

                ChangePasswordWindow pwWindow = new ChangePasswordWindow();

                // Wir setzen den DataContext explizit, falls er nicht schon in der View gesetzt ist.
                // Falls deine View den DataContext im XAML setzt, überschreibt dies hier nichts Kritisches.
                pwWindow.DataContext = pwVm;

                bool? pwResult = pwWindow.ShowDialog();

                if (pwResult != true)
                {
                    Shutdown();
                    return;
                }
            }

            // ------------------------------------------------------------
            // PHASE 3: Initialisierung Hauptfenster
            // -> (Siehe Dokumentation: 01_Start_App.md > Programmablauf Phase 3)
            // ------------------------------------------------------------

            MainWindowViewModel mainVm = new MainWindowViewModel();
            MainWindow mainWindow = new MainWindow();

            mainWindow.DataContext = mainVm;
  
            this.MainWindow = mainWindow;
            mainWindow.Show();

            // ============================================================
            // DER TRICK: Modus jetzt umschalten!
            // ============================================================
            // Jetzt, wo das Hauptfenster offen ist, ändern wir die Regel.
            // Ab jetzt gilt: "Wenn das Hauptfenster zugeht, beende dich."
            // ============================================================
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}