using System;
using System.Windows.Input;

namespace BAT_Man.Services
{
    /// <summary>
    /// Eine Standard-Implementierung des ICommand-Interfaces für MVVM.
    /// Dient als Bindeglied zwischen UI-Elementen und ViewModel-Methoden.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Die auszuführende Aktion
        private readonly Action<object> _execute;

        // Die Bedingung für die Ausführbarkeit (optional)
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Event zur Aktualisierung des Aktivierungsstatus in der UI.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            // Kopplung an den WPF CommandManager für automatische Updates
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Prüft, ob der Befehl ausgeführt werden darf.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Führt die Logik aus.
        /// </summary>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}