// In: App.xaml.cs
using System; // WICHTIG: Für "Exception"
using System.Windows;
using WPF_Test.Services;
using WPF_Test.Views;

namespace WPF_Test
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // ==========================================================
            // HIER BEGINNT DER "FEHLER-FÄNGER" (try-Block)
            // ==========================================================
            try
            {
                // Dein normaler Code...
                base.OnStartup(e);

                LoginWindow loginWindow = new LoginWindow();
                bool? loginResult = loginWindow.ShowDialog();

                if (loginResult != true)
                {
                    Shutdown(); // Beendet die App bei Abbruch
                    return;
                }

                // Dein Check für das Passwort-Ändern
                if (AktiveSitzung.Instance.AngemeldeterTeilnehmer.MussPasswortAendern)
                {
                    ChangePasswordWindow changePwdWindow = new ChangePasswordWindow();
                    bool? changeResult = changePwdWindow.ShowDialog();

                    if (changeResult != true)
                    {
                        AktiveSitzung.Instance.Abmelden();
                        Shutdown();
                        return;
                    }
                }

                // ALLES ERFOLGREICH:
                MainWindow mainWindow = new MainWindow();

                mainWindow.Closed += (s, args) =>
                {
                    Current.Shutdown();
                };

                mainWindow.Show();
            }
            // ==========================================================
            // HIER IST DER "FEHLER-FÄNGER" (catch-Block)
            // ==========================================================
            catch (Exception ex)
            {
                // WENN DIE APP ABSTÜRZT, WIRD DIESER CODE AUSGEFÜHRT
                // Er zeigt uns die GENAUE Fehlermeldung in einer Box.
                MessageBox.Show("FATALER FEHLER BEIM START:\n\n" + ex.ToString(), "Fehler");

                // Beende die App (da sie kaputt ist)
                Shutdown();
            }
        }
    }
}
