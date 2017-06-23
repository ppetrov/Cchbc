using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.iFSA.AgendaModule;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AddActivityModule
{
	public sealed class AddActivityScreenViewModel : ViewModel
	{
		private MainContext MainContext { get; }
		private AddActivityScreenDataProvider DataProvider { get; }
		private IAppNavigator AppNavigator { get; }
		private List<OutletViewModel> AllOutlets { get; } = new List<OutletViewModel>();

		public ObservableCollection<OutletViewModel> Outlets { get; } = new ObservableCollection<OutletViewModel>();
		public ObservableCollection<ActivityTypeCategoryViewModel> Categories { get; } = new ObservableCollection<ActivityTypeCategoryViewModel>();
		public ObservableCollection<ActivityTypeViewModel> Types { get; } = new ObservableCollection<ActivityTypeViewModel>();

		private OutletViewModel _selectedOutlet;
		public OutletViewModel SelectedOutlet
		{
			get { return _selectedOutlet; }
			set { this.SetProperty(ref _selectedOutlet, value); }
		}

		private ActivityTypeCategoryViewModel _selectedCategory;
		public ActivityTypeCategoryViewModel SelectedCategory
		{
			get { return _selectedCategory; }
			set
			{
				this.SetProperty(ref _selectedCategory, value);
				this.Types.Clear();
				if (value != null)
				{
					foreach (var type in value.Types)
					{
						this.Types.Add(type);
					}
				}
				this.SelectedType = this.Types.FirstOrDefault();
			}
		}

		private ActivityTypeViewModel _selectedType;
		public ActivityTypeViewModel SelectedType
		{
			get { return _selectedType; }
			set { this.SetProperty(ref _selectedType, value); }
		}

		private string _search = string.Empty;
		public string Search
		{
			get { return _search; }
			set
			{
				this.SetProperty(ref _search, value);
				this.ApplyCurrentTextSearch();
			}
		}

		private bool _hideSuppressed;
		public bool HideSuppressed
		{
			get { return _hideSuppressed; }
			set
			{
				this.SetProperty(ref _hideSuppressed, value);
				var name = value ? @"HideSuppressedOutlets" : @"ShowSuppressedOutlets";
				this.SwitchSuppressedOutletsCaption = this.MainContext.GetLocalized(new LocalizationKey(nameof(AddActivityScreenViewModel), name));
			}
		}

		private string _switchSuppressedOutletsCaption = string.Empty;
		public string SwitchSuppressedOutletsCaption
		{
			get { return _switchSuppressedOutletsCaption; }
			set { this.SetProperty(ref _switchSuppressedOutletsCaption, value); }
		}

		public ICommand CreateActivityCommand { get; }
		public ICommand StartNewActivityCommand { get; }
		public ICommand SwitchSuppressedOutletsCommand { get; }

		public AddActivityScreenViewModel(MainContext mainContext, AddActivityScreenDataProvider dataProvider, IAppNavigator appNavigator)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));

			this.MainContext = mainContext;
			this.DataProvider = dataProvider;
			this.AppNavigator = appNavigator;
			this.CreateActivityCommand = new RelayCommand(this.CreateActivity);
			this.StartNewActivityCommand = new RelayCommand(this.StartNewActivity);
			this.SwitchSuppressedOutletsCommand = new RelayCommand(this.SwitchSuppressedOutlets);
			this.HideSuppressed = false;
		}

		public void LoadData()
		{
			var feature = new Feature(nameof(AddActivityScreenViewModel), nameof(LoadData));

			try
			{
				using (var ctx = this.MainContext.CreateFeatureContext(feature))
				{
					this.Categories.Clear();
					foreach (var category in this.DataProvider.GetCategories(ctx))
					{
						this.Categories.Add(new ActivityTypeCategoryViewModel(category));
					}

					this.Outlets.Clear();
					foreach (var outlet in this.DataProvider.GetOutlets(ctx))
					{
						this.Outlets.Add(new OutletViewModel(outlet));
					}

					this.AllOutlets.Clear();
					this.AllOutlets.AddRange(this.Outlets);

					ctx.Complete();
				}

				//if (this.AgendaScreenViewModel.SelectedOutletViewModel != null)
				//{
				//	var outlet = this.AgendaScreenViewModel.SelectedOutletViewModel.Outlet;

				//	foreach (var viewModel in this.AllOutlets)
				//	{
				//		if (viewModel.Model == outlet)
				//		{
				//			this.SelectedOutlet = viewModel;
				//			break;
				//		}
				//	}
				//}
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private async void CreateActivity()
		{
			try
			{
				await this.CreateActivity(this.SelectedOutlet, this.SelectedType);
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private async void StartNewActivity()
		{
			var activity = await this.CreateActivity(this.SelectedOutlet, this.SelectedType);
			if (activity != null)
			{
				this.AppNavigator.GoBack();
				this.AppNavigator.NavigateTo(AppScreen.ExecuteActivity, new object[] { @"Agenda", activity });
			}
		}

		private void SwitchSuppressedOutlets()
		{
			this.HideSuppressed = !this.HideSuppressed;
			this.ApplyCurrentTextSearch();
		}

		private void ApplyCurrentTextSearch()
		{
			var search = this.Search;

			this.Outlets.Clear();
			foreach (var viewModel in this.AllOutlets)
			{
				if (this.HideSuppressed && viewModel.Model.IsSuppressed)
				{
					continue;
				}
				if (viewModel.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					this.Outlets.Add(viewModel);
				}
			}
		}

		private async Task<Activity> CreateActivity(OutletViewModel outletViewModel, ActivityTypeViewModel activityTypeViewModel)
		{
			var outlet = default(Outlet);
			if (outletViewModel != null)
			{
				outlet = outletViewModel.Model;
			}
			var activityType = default(ActivityType);
			if (activityTypeViewModel != null)
			{
				activityType = activityTypeViewModel.Model;
			}

			// TODO : Can Create from where to take the reference???
			//var permissionResult = this.Agenda.CanCreate(outlet, activityType);
			//var canContinue = await this.MainContext.CanContinueAsync(permissionResult);
			//if (canContinue)
			//{
			//	using (var ctx = this.MainContext.CreateFeatureContext(null))
			//	{
			//		var activity = this.Agenda.Create(null, outlet, null, null, DateTime.Now);
			//		ctx.Complete();
			//		return activity;
			//	}
			//}

			return null;
		}
	}
}