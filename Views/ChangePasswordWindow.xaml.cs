// Dateipfad: Views/ChangePasswordWindow.xaml.cs

using System.Windows;
using WPF_Test.ViewModels; // WICHTIG: Damit er das ViewModel findet

namespace WPF_Test.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent(); // Wenn der Namespace stimmt, ist dieser Fehler weg!

            // Wir setzen das ViewModel als "Gehirn" ein
            this.DataContext = new ChangePasswordViewModel();
        }
    }
}