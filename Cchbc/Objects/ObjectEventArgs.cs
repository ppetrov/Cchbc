using System;

namespace Cchbc.Objects
{
	public sealed class ObjectEventArgs<T> : EventArgs
	{
		public T ViewModel { get; }

		public ObjectEventArgs(T viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.ViewModel = viewModel;
		}
	}
}