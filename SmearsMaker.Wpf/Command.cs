using System;
using System.Windows.Input;

namespace SmearsMaker.Wpf
{
	public class Command : ICommand
	{
		protected Action Action;
		private readonly bool _canExecute;

		public Command(Action action, bool canExecute = true)
		{
			//  Set the action.
			this.Action = action;
			this._canExecute = canExecute;
		}

		bool ICommand.CanExecute(object parameter)
		{
			return _canExecute;
		}

		public void Execute(object parameter)
		{
			Action?.Invoke();
		}

		public event EventHandler CanExecuteChanged;
	}
}