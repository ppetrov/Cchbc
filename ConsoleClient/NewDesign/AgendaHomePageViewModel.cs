using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Navigation;
using Atos.iFSA.Objects;

namespace ConsoleClient.NewDesign
{
	public sealed class AgendaHomePageViewModel : PageViewModel
	{
		private AgendaScreenViewModel AgendaScreenViewModel { get; set; }
		private List<AgendaOutletViewModel> AllOutlets { get; } = new List<AgendaOutletViewModel>();

		public ICommand DisplaySettingsCommand { get; }
		public ICommand DisplayMapCommand { get; }
		public ICommand DisplayCalendarCommand { get; }

		private string _searchText = string.Empty;
		public string SearchText
		{
			get { return _searchText; }
			set
			{
				this.SetProperty(ref _searchText, value);
				this.ApplyCurrentSearch();
			}
		}

		private AgendaOutletViewModel _selectedOutlet;
		public AgendaOutletViewModel SelectedOutlet
		{
			get { return _selectedOutlet; }
			set
			{
				this.SetProperty(ref _selectedOutlet, value);
				this.LoadSelectedOutlet();
			}
		}
		public ObservableCollection<AgendaOutletViewModel> Outlets { get; } = new ObservableCollection<AgendaOutletViewModel>();

		private OutletImageViewModel _outletImage;
		public OutletImageViewModel OutletImage
		{
			get { return _outletImage; }
			set { this.SetProperty(ref _outletImage, value); }
		}

		public ObservableCollection<PerformanceIndicatorViewModel> PerformanceIndicators { get; } = new ObservableCollection<PerformanceIndicatorViewModel>();

		private AgendaActivityTabViewModel _selectedActivityTab;
		public AgendaActivityTabViewModel SelectedActivityTab
		{
			get { return _selectedActivityTab; }
			set
			{
				this.SetProperty(ref _selectedActivityTab, value);
				this.LoadSelectedActivityTab();
			}
		}
		public ObservableCollection<AgendaActivityTabViewModel> ActivityTabs { get; } = new ObservableCollection<AgendaActivityTabViewModel>();

		public AgendaHomePageViewModel(MainContext mainContext)
			: base(mainContext)
		{
			this.DisplaySettingsCommand = new ActionCommand(async () =>
			{
				var feature = new Feature(nameof(AgendaHomePageViewModel), nameof(DisplaySettingsCommand));
				try
				{
					this.MainContext.Save(feature);
					await this.MainContext.GetService<INavigationService>().NavigateToAsync<AgendaMapPageViewModel>();
				}
				catch (Exception ex)
				{
					this.MainContext.Save(feature, ex);
				}
			});
			this.DisplayMapCommand = new ActionCommand(async () =>
			{
				var feature = new Feature(nameof(AgendaHomePageViewModel), nameof(DisplayMapCommand));
				try
				{
					this.MainContext.Save(feature);
					await this.MainContext.GetService<INavigationService>().NavigateToAsync<AgendaMapPageViewModel>();
				}
				catch (Exception ex)
				{
					this.MainContext.Save(feature, ex);
				}
			});
			this.DisplayCalendarCommand = new ActionCommand(() =>
			{
				// TODO : Display calendar control !!!
			});
		}

		private DateTime _currentDate = DateTime.Today;
		public DateTime CurrentDate
		{
			get { return _currentDate; }
			set { this.SetProperty(ref _currentDate, value); }
		}

		public override Task InitializeAsync(object parameter)
		{
			this.AgendaScreenViewModel = parameter as AgendaScreenViewModel;

			this.AllOutlets.Clear();
			foreach (var outlet in this.MainContext.GetService<IAgendaHomeScreenDataProvider>().GetOutlets(this.CurrentDate))
			{
				this.AllOutlets.Add(new AgendaOutletViewModel(outlet));
			}

			// Set the search text to display outlets
			this.SearchText = string.Empty;

			// Select the first outlet to display details
			this.SelectedOutlet = this.Outlets.FirstOrDefault();

			return base.InitializeAsync(parameter);
		}

		private void ApplyCurrentSearch()
		{
			this.Outlets.Clear();

			foreach (var viewModel in this.AllOutlets)
			{
				if (viewModel.Name.IndexOf(this.SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					this.Outlets.Add(viewModel);
				}
			}
		}

		private void LoadSelectedOutlet()
		{
			this.OutletImage = null;
			this.PerformanceIndicators.Clear();

			if (this.SelectedOutlet != null)
			{
				var outlet = this.SelectedOutlet.Outlet;

				var image = this.MainContext.GetService<IAgendaHomeScreenDataProvider>().GetDefaultImage(outlet);
				if (image != null)
				{
					this.OutletImage = new OutletImageViewModel(image);
				}

				var indicators = this.MainContext.GetService<IAgendaHomeScreenDataProvider>().GetPerformanceIndicators(outlet);
				foreach (var indicator in indicators)
				{
					this.PerformanceIndicators.Add(new PerformanceIndicatorViewModel(indicator));
				}
			}

			// Display activities by default
			this.SelectedActivityTab = this.ActivityTabs.FirstOrDefault();
		}

		private ActivityViewModel _selectedActivity;
		public ActivityViewModel SelectedActivity
		{
			get { return _selectedActivity; }
			set { this.SetProperty(ref _selectedActivity, value); }
		}
		public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();
		public ObservableCollection<ActivityViewModel> LongTermActivities { get; } = new ObservableCollection<ActivityViewModel>();
		public ObservableCollection<ActivityViewModel> AllActivities { get; } = new ObservableCollection<ActivityViewModel>();

		private void LoadSelectedActivityTab()
		{
			this.SelectedActivity = null;

			this.Activities.Clear();
			this.LongTermActivities.Clear();
			this.AllActivities.Clear();

			if (this.SelectedOutlet != null)
			{
				var outlet = this.SelectedOutlet.Outlet;

				switch (this.SelectedActivityTab.Type)
				{
					case AgendaActivityTabType.Today:
						foreach (var activity in this.MainContext.GetService<IAgendaHomeScreenDataProvider>().GetActivities(outlet))
						{
							this.Activities.Add(new ActivityViewModel(activity));
						}
						break;
					case AgendaActivityTabType.LongTerm:
						foreach (var activity in this.MainContext.GetService<IAgendaHomeScreenDataProvider>().GetLongTermActivities(outlet))
						{
							this.LongTermActivities.Add(new ActivityViewModel(activity));
						}
						break;
					case AgendaActivityTabType.ViewAll:
						foreach (var activity in this.MainContext.GetService<IAgendaHomeScreenDataProvider>().GetLongTermActivities(outlet))
						{
							this.LongTermActivities.Add(new ActivityViewModel(activity));
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}


	}

	public sealed class ActivityViewModel
	{
		public Activity Activity { get; }

		public ActivityViewModel(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));
			Activity = activity;
		}
	}

	public interface IAgendaHomeScreenDataProvider
	{
		IEnumerable<Outlet> GetOutlets(DateTime dateTime);
		IEnumerable<PerformanceIndicator> GetPerformanceIndicators(Outlet outlet);
		IEnumerable<Activity> GetActivities(Outlet outlet);
		IEnumerable<Activity> GetLongTermActivities(Outlet outlet);
		OutletImage GetDefaultImage(Outlet outlet);
	}


}