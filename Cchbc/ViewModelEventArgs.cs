﻿using System;

namespace Atos
{
	public sealed class ViewModelEventArgs<T> : EventArgs
	{
		public T ViewModel { get; }

		public ViewModelEventArgs(T viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.ViewModel = viewModel;
		}
	}
}