// Dateipfad: Views/FirmenUebersichtView.xaml.cs

// Imports für die Basis-Funktionen
using System.Windows.Controls;
// Imports für unsere ViewModels (wird später vielleicht gebraucht)
using WPF_Test.ViewModels;

// Der Namespace MUSS so lauten (wegen des Ordners 'Views')
namespace WPF_Test.Views
{
    /// <summary>
    /// Interaktionslogik für FirmenUebersichtView.xaml
    /// 
    /// WICHTIG: Die Klasse MUSS 'FirmenUebersichtView' heißen
    /// und von 'UserControl' erben.
    /// </summary>
    public partial class FirmenUebersichtView : UserControl
    {
        /// <summary>
        /// Konstruktor
        /// </summary>
        public FirmenUebersichtView()
        {
            // Dieser Befehl wird jetzt (nach der Korrektur)
            // gefunden und ist nicht mehr rot.
            InitializeComponent();
        }
    }
}