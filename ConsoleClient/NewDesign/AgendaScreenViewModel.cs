using System;
using System.Collections.ObjectModel;
using Atos.Client;
using Atos.Client.Navigation;

namespace ConsoleClient.NewDesign
{
	public sealed class AgendaScreenViewModel : ViewModel
	{
		public MainContext MainContext { get; }
		public ObservableCollection<AgendaTabViewModel> Tabs { get; } = new ObservableCollection<AgendaTabViewModel>();

		private AgendaTabViewModel _selectedAgendaTab;
		public AgendaTabViewModel SelectedAgendaTab
		{
			get { return _selectedAgendaTab; }
			set
			{
				if (_selectedAgendaTab == value)
				{
					return;
				}
				this.SetProperty(ref _selectedAgendaTab, value);

				switch (value.Category)
				{
					case AgendaTabCategory.Home:
						this.MainContext.GetService<INavigationService>().NavigateToAsync<AgendaHomeScreenViewModel>(this);
						break;
					case AgendaTabCategory.Info:
						this.MainContext.GetService<INavigationService>().NavigateToAsync<AgendaInfoScreenViewModel>(this);
						break;
					case AgendaTabCategory.History:
						break;
					case AgendaTabCategory.Promo:
						break;
					case AgendaTabCategory.Notes:
						break;
					case AgendaTabCategory.Docs:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public AgendaScreenViewModel(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
			this.Tabs.Add(new AgendaTabViewModel(new AgendaTab(@"Home", AgendaTabCategory.Home)));
			this.Tabs.Add(new AgendaTabViewModel(new AgendaTab(@"Docs", AgendaTabCategory.Docs)));
		}
	}
}