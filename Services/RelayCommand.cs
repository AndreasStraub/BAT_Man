// Dateipfad: Services/RelayCommand.cs
// Dies ist eine Standard-Hilfsklasse für MVVM

using System;
using System.Windows.Input;

namespace WPF_Test.Services
{
    /// <summary>
    /// Eine wiederverwendbare Klasse, die ICommand implementiert.
    /// Sie leitet den 'Execute'-Aufruf (den Klick) an eine
    /// C#-Methode (ein 'Delegate') im ViewModel weiter.
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Die Methode, die bei "Klick" ausgeführt wird
        private readonly Action<object> _execute;

        // Die Methode, die prüft, OB geklickt werden darf
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            // Wenn keine 'canExecute'-Regel da ist, ist der Button immer klickbar
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            // Führe die übergebene Methode aus
            _execute(parameter);
        }
    }
}