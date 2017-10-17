using System;
using System.Threading.Tasks;

namespace Atos.Client
{
	public abstract class ScreenViewModel : ViewModel
	{
		private string _title = string.Empty;
		public string Title
		{
			get { return _title; }
			set { this.SetProperty(ref _title, value); }
		}
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