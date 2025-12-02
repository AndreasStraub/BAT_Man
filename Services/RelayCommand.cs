using System;
using System.Windows.Input;

namespace BAT_Man.Services
{
    /// <summary>
    /// Eine Standard-Implementierung des ICommand-Interfaces für MVVM.
    /// <para>
    /// FUNKTION:
    /// Diese Klasse fungiert als "Brücke" zwischen einem UI-Element (z.B. Button)
    /// und einer Methode im ViewModel. Sie erlaubt es, Logik auszuführen (Execute)
    /// und die Aktivierung zu steuern (CanExecute), ohne Code-Behind zu nutzen.
    /// </para>
    /// </summary>
    public class RelayCommand : ICommand
    {
        // --- Private Felder ---

        // Die Methode, die ausgeführt wird (die "Arbeit").
        // Action<object> ist ein Delegat für eine Methode, die einen Parameter nimmt und nichts zurückgibt (void).
        private readonly Action<object> _execute;

        // Die Methode, die prüft, ob die Arbeit getan werden darf (die "Regel").
        // Predicate<object> ist ein Delegat, der einen Parameter nimmt und bool zurückgibt.
        private readonly Predicate<object> _canExecute;

        // --- ICommand Implementierung ---

        /// <summary>
        /// Das Event, das von der UI abonniert wird.
        /// Wenn dieses Event feuert, fragt der Button beim Command nach: "Darf ich noch geklickt werden?".
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            // Hier wird der CommandManager von WPF "angezapft".
            // Er sorgt dafür, dass die Buttons ihren Status automatisch aktualisieren,
            // sobald der Benutzer interagiert (Klick, Fokuswechsel, Tastatur).
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="execute">Die Methode, die beim Klick ausgeführt werden soll.</param>
        /// <param name="canExecute">Optional: Eine Methode, die prüft, ob der Button aktiv ist.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            // Prüfung: Ein Command ohne Aktion macht keinen Sinn.
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Prüft, ob der Befehl aktuell ausgeführt werden darf.
        /// Wird vom Button (oder der UI) automatisch aufgerufen.
        /// </summary>
        /// <param name="parameter">Optionaler Parameter aus der View (CommandParameter).</param>
        /// <returns>True = Button ist aktiv (Enabled); False = Button ist ausgegraut (Disabled).</returns>
        public bool CanExecute(object parameter)
        {
            // Wenn keine Regel definiert wurde (_canExecute ist null), ist der Befehl immer erlaubt.
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Führt die hinterlegte Logik aus.
        /// Wird aufgerufen, wenn der Benutzer klickt.
        /// </summary>
        /// <param name="parameter">Optionaler Parameter aus der View.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}