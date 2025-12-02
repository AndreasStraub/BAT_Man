// Dateipfad: Views/LoginWindow.xaml.cs

using System.Windows;

// WICHTIG: Namespace anpassen, falls du die Datei verschoben hast
namespace BAT_Man.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Wir setzen den DataContext NICHT hier, 
            // sondern in der App.xaml.cs, wenn wir das Fenster erstellen.
            // Das hält das Fenster sauber.
        }
    }
}