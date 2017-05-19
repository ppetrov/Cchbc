using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Logs;
using Cchbc.Validation;
using iFSA.AgendaModule.Objects;
using iFSA.AgendaModule.ViewModels;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule
{
	public sealed class AgendaScreenViewModel : ViewModel
	{
		private Agenda Agenda { get; }
		private IAppNavigator AppNavigator { get; }
		private IUIThreadDispatcher UIThreadDispatcher { get; }
		private MainContext MainContext { get; }
		private List<AgendaOutletViewModel> AllOutlets { get; } = new List<AgendaOutletViewModel>();

		private DateTime _currentDate;
		public DateTime CurrentDate
		{
			get { return _currentDate; }
			set { this.SetProperty(ref _currentDate, value); }
		}

		private User User { get; set; }

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

		private AgendaOutletViewModel _selectedOutletViewModel;
		public AgendaOutletViewModel SelectedOutletViewModel
		{
			get { return _selectedOutletViewModel; }
			set { this.SetProperty(ref _selectedOutletViewModel, value); }
		}

		public ICommand PreviousDayCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand DisplayCalendarCommand { get; }
		public ICommand DisplayAddActivityCommand { get; }
		public ICommand RemoveActivityCommand { get; }

		public ObservableCollection<AgendaOutletViewModel> Outlets { get; } = new ObservableCollection<AgendaOutletViewModel>();

		public AgendaScreenViewModel(MainContext mainContext, Agenda agenda, IAppNavigator appNavigator, IUIThreadDispatcher uiThreadDispatcher)
		{
			if (agenda == null) throw new ArgumentNullException(nameof(agenda));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));
			if (uiThreadDispatcher == null) throw new ArgumentNullException(nameof(uiThreadDispatcher));

			this.Agenda = agenda;
			this.AppNavigator = appNavigator;
			this.UIThreadDispatcher = uiThreadDispatcher;
			this.MainContext = mainContext;
			this.PreviousDayCommand = new RelayCommand(this.LoadPreviousDay);
			this.NextDayCommand = new RelayCommand(this.LoadNextDay);
			this.DisplayCalendarCommand = new RelayCommand(this.DisplayCalendar);
			this.DisplayAddActivityCommand = new RelayCommand(this.DisplayAddActivity);
			this.RemoveActivityCommand = new RelayCommand(this.RemoveActivity);

			this.Agenda.ActivityAdded += this.ActivityAdded;
			this.Agenda.OutletImageDownloaded += this.OutletImageDownloaded;
		}

		private void OutletImageDownloaded(object sender, OutletImageEventArgs e)
		{
			try
			{
				var outletImage = e.OutletImage;
				var number = outletImage.Outlet;
				lock (this)
				{
					foreach (var viewModel in this.AllOutlets)
					{
						if (viewModel.Number == number)
						{
							var match = viewModel;
							this.UIThreadDispatcher.Dispatch(() =>
							{
								match.OutletImage = DateTime.Now.ToString(@"G");
							});
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private void ActivityAdded(object sender, ActivityEventArgs e)
		{
			var activity = e.Activity;
			var outlet = activity.Outlet;

			var outletViewModel = default(AgendaOutletViewModel);

			// Search in all outlets
			lock (this)
			{
				foreach (var viewModel in this.AllOutlets)
				{
					if (outlet == viewModel.Outlet)
					{
						outletViewModel = viewModel;
						break;
					}
				}
			}

			// Not found
			if (outletViewModel == null)
			{
				// Create it
				outletViewModel = new AgendaOutletViewModel(this.MainContext, new AgendaOutlet(outlet, new List<Activity>()));

				// Add it to the list
				this.AllOutlets.Add(outletViewModel);
			}

			// Add the new activity
			outletViewModel.Activities.Add(new ActivityViewModel(outletViewModel, activity));

			// TODO : !!! Sort the collection

			// TODO : !!! Filter the collection
			this.ApplyCurrentTextSearch();
		}

		public void LoadDay(User user, DateTime dateTime)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			var feature = Feature.StartNew(nameof(AgendaScreenViewModel), nameof(LoadDay));
			try
			{
				this.LoadData(user, dateTime);
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
				this.MainContext.FeatureManager.Save(feature, ex);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature, dateTime.ToString(@"O"));
			}
		}

		public PermissionResult CanCreate(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			return this.Agenda.CanCreate(activity);
		}

		public Activity CreateActivity(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			return this.Agenda.Create(activity);
		}

		private void LoadNextDay()
		{
			var feature = Feature.StartNew(nameof(AgendaScreenViewModel), nameof(LoadNextDay));
			try
			{
				this.LoadData(this.CurrentDate.AddDays(-1));
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
				this.MainContext.FeatureManager.Save(feature, ex);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
		}

		private void LoadPreviousDay()
		{
			var feature = Feature.StartNew(nameof(AgendaScreenViewModel), nameof(LoadPreviousDay));
			try
			{
				this.LoadData(this.CurrentDate.AddDays(-1));
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
				this.MainContext.FeatureManager.Save(feature, ex);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
		}

		private void ApplyCurrentTextSearch()
		{
			var search = this.Search;

			this.Outlets.Clear();
			lock (this)
			{
				foreach (var viewModel in this.AllOutlets)
				{
					if (viewModel.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						this.Outlets.Add(viewModel);
					}
				}
			}
		}

		private void DisplayCalendar()
		{
			// TODO : Probably it's better to pass Agenda as reference
			// Close/Cancel is performed via Agenda ONLY, or maybe not
			// How to close/cancel multiple days ???
			this.AppNavigator.NavigateTo(AppScreen.Calendar, this.CurrentDate);
		}

		private void DisplayAddActivity()
		{
			this.AppNavigator.NavigateTo(AppScreen.AddActivity, this);
		}

		private void RemoveActivity()
		{
			// TODO : !!! Add support for Delete
		}

		private void LoadData(DateTime dateTime)
		{
			this.LoadData(this.User, dateTime);
		}

		private void LoadData(User user, DateTime dateTime)
		{
			this.User = user;
			this.CurrentDate = dateTime;
			this.Agenda.LoadDay(this.MainContext, user, dateTime);

			this.Outlets.Clear();
			lock (this)
			{
				this.AllOutlets.Clear();
				foreach (var outlet in this.Agenda.Outlets)
				{
					var viewModel = new AgendaOutletViewModel(this.MainContext, outlet);
					this.AllOutlets.Add(viewModel);
					this.Outlets.Add(viewModel);
				}
			}

		}
	}


	public sealed class AddActivityScreenViewModel : ViewModel
	{
		private AgendaScreenViewModel AgendaScreenViewModel { get; }
		private MainContext MainContext { get; }

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

		public ICommand CreateActivityCommand { get; }
		public ICommand StartNewActivityCommand { get; }

		public AddActivityScreenViewModel(AgendaScreenViewModel agendaScreenViewModel, MainContext mainContext)
		{
			if (agendaScreenViewModel == null) throw new ArgumentNullException(nameof(agendaScreenViewModel));
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.AgendaScreenViewModel = agendaScreenViewModel;
			this.MainContext = mainContext;
			this.CreateActivityCommand = new RelayCommand(this.CreateActivity);
			this.StartNewActivityCommand = new RelayCommand(this.StartNewActivity);
		}


		public void Load()
		{
			// TODO : !!! Load Outlets
			var outlets = new Outlet[100];

			// TODO : !!! Load categories

			this.Outlets.Clear();
			this.AllOutlets.Clear();
			foreach (var outlet in outlets)
			{
				var outletViewModel = new OutletViewModel(outlet);

				this.Outlets.Add(outletViewModel);
				this.AllOutlets.Add(outletViewModel);
			}

			if (this.AgendaScreenViewModel.SelectedOutletViewModel != null)
			{
				var outlet = this.AgendaScreenViewModel.SelectedOutletViewModel.Outlet;

				foreach (var viewModel in this.AllOutlets)
				{
					if (viewModel.Model == outlet)
					{
						this.SelectedOutlet = viewModel;
						break;
					}
				}
			}
		}

		private async void CreateActivity()
		{
			try
			{
				var activity = default(Activity);
				await CreateActivity(activity);
			}
			catch (Exception ex)
			{
				this.MainContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private async void StartNewActivity()
		{
			var activity = default(Activity);
			activity = await CreateActivity(activity);

			if (activity != null)
			{
				
				// TODO : !!!
				var nav = default(IAppNavigator);
				nav.GoBack();
				nav.NavigateTo(AppScreen.AddActivity, activity);
			}
		}

		private async Task<Activity> CreateActivity(Activity activity)
		{
			var createActivity = false;
			var permissionResult = this.AgendaScreenViewModel.CanCreate(activity);
			var type = permissionResult.Type;
			switch (type)
			{
				case PermissionType.Allow:
					createActivity = true;
					break;
				case PermissionType.Confirm:
					var confirmation = await this.MainContext.ModalDialog.ShowAsync(permissionResult.LocalizationKeyName, Feature.None, type);
					if (confirmation == DialogResult.Accept)
					{
						createActivity = true;
					}
					break;
				case PermissionType.Deny:
					await this.MainContext.ModalDialog.ShowAsync(permissionResult.LocalizationKeyName, Feature.None, type);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			if (createActivity)
			{
				return this.AgendaScreenViewModel.CreateActivity(activity);
			}
			return null;
		}

		private void ApplyCurrentTextSearch()
		{
			var search = this.Search;

			this.Outlets.Clear();
			foreach (var viewModel in this.AllOutlets)
			{
				if (viewModel.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					this.Outlets.Add(viewModel);
				}
			}
		}
	}

	public sealed class OutletViewModel : ViewModel<Outlet>
	{
		public string Number { get; }
		public string Name { get; }

		public OutletViewModel(Outlet model) : base(model)
		{
			this.Number = model.Id.ToString();
			this.Name = model.Name;
		}
	}

	public sealed class ActivityTypeCategoryViewModel : ViewModel<ActivityTypeCategory>
	{
		public string Name { get; }
		public ObservableCollection<ActivityTypeViewModel> Types { get; } = new ObservableCollection<ActivityTypeViewModel>();

		public ActivityTypeCategoryViewModel(ActivityTypeCategory model) : base(model)
		{
			this.Name = model.Name;
			foreach (var type in model.Types)
			{
				this.Types.Add(new ActivityTypeViewModel(type));
			}
		}
	}

	public sealed class ActivityTypeViewModel : ViewModel<ActivityType>
	{
		public string Name { get; set; }

		public ActivityTypeViewModel(ActivityType model) : base(model)
		{
		}
	}
}