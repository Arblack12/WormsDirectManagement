using System;
using System.Windows.Input;

namespace WormsDirectManagement.Helpers
{
    internal class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _can;

        public RelayCommand(Action execute, Func<bool>? can = null)
        {
            _execute = execute;
            _can = can;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => _can?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();

        public void Raise() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
