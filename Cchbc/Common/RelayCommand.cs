﻿using System;
using System.Windows.Input;

namespace Cchbc.Common
{
	public sealed class RelayCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		public event EventHandler CanExecuteChanged;

		public RelayCommand(Action execute)
			: this(execute, null)
		{
		}

		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null) throw new ArgumentNullException(nameof(execute));

			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute?.Invoke() ?? true;
		}

		public void Execute(object parameter)
		{
			_execute();
		}

		public void RaiseCanExecuteChanged()
		{
			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}