using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Features;
using Cchbc.Logs;
using iFSA.AddActivityModule;
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
			this.Agenda.ActivityUpdated += this.ActivityUpdated;
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
				outletViewModel = new AgendaOutletViewModel(this, new AgendaOutlet(outlet, new List<Activity>()));

				// Add it to the list
				this.AllOutlets.Add(outletViewModel);
			}

			// Add the new activity
			outletViewModel.Activities.Add(new ActivityViewModel(outletViewModel, activity));

			// TODO : !!! Sort the collection

			this.ApplyCurrentTextSearch();
		}

		private void ActivityUpdated(object sender, ActivityEventArgs e)
		{
			var activity = e.Activity;
			var outlet = activity.Outlet;
			var acitivityId = activity.Id;

			// Search in all outlets
			lock (this)
			{
				foreach (var viewModel in this.AllOutlets)
				{
					if (outlet == viewModel.Outlet)
					{
						var activities = viewModel.Activities;
						for (var index = 0; index < activities.Count; index++)
						{
							var activityViewModel = activities[index];
							if (activityViewModel.Model.Id == acitivityId)
							{
								activities[index] = new ActivityViewModel(viewModel, activity);
								break;
							}
						}
						break;
					}
				}
			}

			// TODO : !!! Sort the collection
			this.ApplyCurrentTextSearch();
		}

		public void LoadDay(User user, DateTime dateTime)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(LoadDay));
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

		public async Task<Activity> CreateActivity(OutletViewModel outletViewModel, ActivityTypeViewModel activityTypeViewModel)
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
			var permissionResult = this.Agenda.CanCreateActivity(outlet, activityType);
			var canContinue = await this.MainContext.CanContinueAsync(permissionResult);
			if (canContinue)
			{
				var activityStatus = DataHelper.GetOpenActivityStatus(this.MainContext);

				var date = DateTime.Now;
				var activity = new Activity(0, outlet, activityType, activityStatus, date, date, string.Empty);
				return this.Agenda.Create(activity);
			}
			return null;
		}

		private void LoadNextDay()
		{
			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(LoadNextDay));
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
			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(LoadPreviousDay));
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
					var viewModel = new AgendaOutletViewModel(this, outlet);
					this.AllOutlets.Add(viewModel);
					this.Outlets.Add(viewModel);
				}
			}
		}

		public async Task ChangeStartTimeAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			// TODO : From constructor
			var timeSelector = default(ITimeSelector);

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(ChangeStartTimeAsync));
			try
			{
				var activity = activityViewModel.Model;
				await timeSelector.ShowAsync(
					dateTime => this.Agenda.CanChangeStartTime(activity, dateTime),
					dateTime => this.Agenda.ChangeStartTime(activity, dateTime));
			}
			catch (Exception ex)
			{
				this.MainContext.FeatureManager.Save(feature, ex);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
		}

		public async Task CancelAsync(ActivityViewModel activityViewModel)
		{
			// TODO : From constructor
			var cancelReasonSelector = default(IActivityCancelReasonSelector);

			var feature = new Feature(nameof(AgendaScreenViewModel), nameof(CancelAsync));
			try
			{
				var activity = activityViewModel.Model;
				await cancelReasonSelector.ShowAsync(activity,
					cancelReason => this.Agenda.CanCancel(activity),
					cancelReason => this.Agenda.Cancel(activity, cancelReason));
			}
			catch (Exception ex)
			{
				this.MainContext.FeatureManager.Save(feature, ex);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
		}

		public async Task CloseAsync(ActivityViewModel activityViewModel)
		{
			throw new NotImplementedException();
		}
	}
}