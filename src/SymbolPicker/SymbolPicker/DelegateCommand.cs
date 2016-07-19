namespace SymbolPicker
{
    using System;
    using System.Windows.Input;

    public sealed class DelegateCommand : ICommand
    {
#pragma warning disable SA1309 // Field names must not begin with underscore
        private readonly Action<object> _action;

#pragma warning restore SA1309 // Field names must not begin with underscore
        public DelegateCommand(Action<object> action)
        {
            this._action = action;
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this._action(parameter);
        }
    }
}