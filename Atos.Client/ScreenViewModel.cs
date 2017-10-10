using System;
using System.Threading.Tasks;

namespace Atos.Client
{
	public abstract class ScreenViewModel : ViewModel
	{
		public MainContext MainContext { get; }

		private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			set { this.SetProperty(ref _isBusy, value); }
		}

		protected ScreenViewModel(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public virtual Task InitializeAsync(object parameter)
		{
			return Task.FromResult(true);
		}
	}
}