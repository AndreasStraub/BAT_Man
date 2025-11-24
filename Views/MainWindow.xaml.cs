// Views/MainWindow.xaml.cs
using System.Windows;
// Das ViewModel-Binding ist jetzt in XAML, 'using' wird nicht mehr gebraucht

namespace WPF_Test.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Die Zeile "this.DataContext = new MainWindowViewModel();"
            // kann jetzt gelöscht werden, da wir es in XAML gemacht haben.
        }

        // Die Theme-Switcher-Logik (wird später auch noch verschoben)
        private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            // TODO: Diese Logik muss ins ViewModel
        }
    }
}