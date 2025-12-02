// Dateipfad: Views/FirmaAnlegenView.xaml.cs

using System.Windows.Controls; // WICHTIG: 'Window' zu 'UserControl' geändert

// WICHTIG: Der Namespace MUSS so lauten
namespace BAT_Man.Views
{
    /// <summary>
    /// Interaktionslogik für FirmaAnlegenView.xaml
    /// 
    /// REPARATUR: Erbt jetzt wieder von 'UserControl'.
    /// Der Code-Behind ist (im MVVM-Muster) fast leer.
    /// </summary>
    public partial class FirmaAnlegenView : UserControl
    {
        public FirmaAnlegenView()
        {
            InitializeComponent();

            // HINWEIS: Wir weisen das "Gehirn" (ViewModel)
            // jetzt im MainWindowViewModel zu (beim Klick),
            // damit die Seite bei jedem Klick "frisch" (leer) ist.
        }
    }
}