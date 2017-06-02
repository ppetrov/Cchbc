using System;

namespace Atos.Client
{
	public sealed class ViewModelEventArgs<T> : EventArgs where T : ViewModel
	{
		public T ViewModel { get; }

		public ViewModelEventArgs(T viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.ViewModel = viewModel;
		}
	}
}