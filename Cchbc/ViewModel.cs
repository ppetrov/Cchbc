﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Atos
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
				this.OnPropertyChanged(propertyName);
			}
		}
	}

	public class ViewModel<T> : ViewModel
	{
		public T Model { get; }

		public ViewModel(T model)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));

			this.Model = model;
		}
	}
}