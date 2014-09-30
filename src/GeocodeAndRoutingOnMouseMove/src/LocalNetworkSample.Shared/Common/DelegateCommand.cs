using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LocalNetworkSample.Common
{
	internal class DelegateCommand : ICommand
	{
		Action command;
		Action<object> commandWithParameter;
		Func<object, bool> canExecute;
		public DelegateCommand(Action command)
		{
			this.command = command;
		}
		public DelegateCommand(Action<object> command)
		{
			commandWithParameter = command;
		}
		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute) : this(execute)
		{
			this.canExecute = canExecute;
		}
		public bool CanExecute(object parameter)
		{
			if (canExecute != null)
				return canExecute(parameter);
			if(commandWithParameter != null || command != null)
				return true;
			return false;
		}

		public event EventHandler CanExecuteChanged;
		public void OnCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		public void Execute(object parameter)
		{
			if (commandWithParameter != null)
				commandWithParameter(parameter);
			else
				command();
		}
	}
}
