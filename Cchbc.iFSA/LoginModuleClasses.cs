using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.iFSA.LoginModule;
using Cchbc.iFSA.Objects;
using Cchbc.Localization;
using Cchbc.Logs;

namespace Cchbc.iFSA
{
	public sealed class LoginScreen
	{
		public LoginScreenViewModel ViewModel { get; } = new LoginScreenViewModel(GlobalAppContext.AppContext, GlobalAppContext.AppNavigator, null, null, null, null);

		public async void LoadData()
		{
			try
			{
				await this.ViewModel.LoadDataAsync();
			}
			catch (Exception ex)
			{
				GlobalAppContext.AppContext.Log(ex.ToString(), LogLevel.Error);
			}
		}
	}

	public sealed class AgendaScreen
	{
		public AgendaViewModel ViewModel { get; } = new AgendaViewModel(GlobalAppContext.AppContext, GlobalAppContext.Agenda, GlobalAppContext.AppNavigator);

		public void LoadData()
		{

		}
	}

	public sealed class Agenda
	{
		public List<Visit> Visits { get; }
		public User User { get; private set; }
		public DateTime CurrentDate { get; private set; }

		private Func<AppContext, User, DateTime, List<Visit>> DataProvider { get; }

		public Agenda(Func<AppContext, User, DateTime, List<Visit>> dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void LoadDay(AppContext appContext, User user, DateTime dateTime)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.CurrentDate = dateTime;

			this.Visits.Clear();
			this.Visits.AddRange(this.DataProvider(appContext, user, dateTime));
		}

		public void LoadNextDay(AppContext appContext)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));

			this.LoadDay(appContext, this.User, this.CurrentDate.AddDays(1));
		}

		public void LoadPreviousDay(AppContext appContext)
		{
			this.LoadDay(appContext, this.User, this.CurrentDate.AddDays(-1));
		}
	}

	public sealed class Calendar
	{

	}

	public sealed class AgendaViewModel : ViewModel
	{
		private Agenda Agenda { get; }
		private IAppNavigator AppNavigator { get; }
		private AppContext AppContext { get; }

		private DateTime _currentDate;
		public DateTime CurrentDate
		{
			get { return _currentDate; }
			set { this.SetProperty(ref _currentDate, value); }
		}

		public ICommand PreviousDayCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand DisplayCalendarCommand { get; }

		public ObservableCollection<AgendaOutletViewModel> Outlets { get; } = new ObservableCollection<AgendaOutletViewModel>();

		public AgendaViewModel(AppContext appContext, Agenda agenda, IAppNavigator appNavigator)
		{
			if (agenda == null) throw new ArgumentNullException(nameof(agenda));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));

			this.Agenda = agenda;
			this.AppNavigator = appNavigator;
			this.AppContext = appContext;
			this.PreviousDayCommand = new RelayCommand(() =>
			{
				this.Agenda.LoadPreviousDay(this.AppContext);
				this.SetupData();
			});
			this.NextDayCommand = new RelayCommand(() =>
			{
				this.Agenda.LoadNextDay(this.AppContext);
				this.SetupData();
			});
			this.DisplayCalendarCommand = new RelayCommand(() =>
			{
				// TODO : Probably it's better to pass Agenda as reference
				// Close/Cancel is performed via Agenda ONLY

				// How to close/cancel multiple days ???
				this.AppNavigator.NavigateTo(AppScreen.Calendar, this.CurrentDate);
			});
		}

		public void LoadData(User user, DateTime dateTime)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.Agenda.LoadDay(this.AppContext, user, dateTime);
			this.SetupData();
		}

		public void LoadData(List<Visit> visits)
		{
			if (visits == null) throw new ArgumentNullException(nameof(visits));

			this.SetupData(visits);
		}

		private void SetupData()
		{
			this.SetupData(this.Agenda.Visits);
		}

		private void SetupData(IEnumerable<Visit> visits)
		{
			this.Outlets.Clear();
			foreach (var byOutlet in visits.GroupBy(v => v.Outlet))
			{
				var outlet = byOutlet.Key;
				var activities = byOutlet.SelectMany(v => v.Activities).ToList();

				this.Outlets.Add(new AgendaOutletViewModel(null, new AgendaOutlet(outlet, activities)));
			}

			// TODO : Sort outlets & activities
		}
	}






















	public static class DataHelper
	{
		public static TradeChannel GetTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			TradeChannel result;
			cache.GetValues<TradeChannel>(context).TryGetValue(-1, out result);

			return result ?? TradeChannel.Empty;
		}

		public static SubTradeChannel GetSubTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			SubTradeChannel result;
			cache.GetValues<SubTradeChannel>(context).TryGetValue(-1, out result);

			return result ?? SubTradeChannel.Empty;
		}
	}












	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }
		public List<OutletAddress> Addresses { get; } = new List<OutletAddress>();

		public Outlet(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class OutletAddress
	{
		public long Outlet { get; }
		public long Id { get; }
		public string Street { get; }
		public string Number { get; }
		public string City { get; }

		public OutletAddress(long outlet, long id, string street, string number, string city)
		{
			Outlet = outlet;
			Id = id;
			Street = street;
			Number = number;
			City = city;
		}
	}

	public sealed class TradeChannel
	{
		public static readonly TradeChannel Empty = new TradeChannel(0, string.Empty, string.Empty);

		public long Id { get; }
		public string Name { get; }
		public string Description { get; }

		public TradeChannel(long id, string name, string description)
		{
			this.Id = id;
			this.Name = name;
			this.Description = description;
		}
	}

	public sealed class SubTradeChannel
	{
		public static readonly SubTradeChannel Empty = new SubTradeChannel(0, string.Empty, string.Empty);

		public long Id { get; }
		public string Name { get; }
		public string Description { get; }

		public SubTradeChannel(long id, string name, string description)
		{
			this.Id = id;
			this.Name = name;
			this.Description = description;
		}
	}


	public sealed class Article
	{
		public long Id { get; }
		public string Name { get; }
		public Brand Brand { get; }
		public Flavor Flavor { get; }
	}

	public sealed class Brand
	{
		public static readonly Brand Empty = new Brand(0, string.Empty);

		public long Id { get; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class Flavor
	{
		public static readonly Flavor Empty = new Flavor(0, string.Empty);

		public long Id { get; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}




	public sealed class AgendaOutletViewModel : ViewModel<AgendaOutlet>
	{
		public AppContext Context { get; }

		public string Number { get; }
		public string Name { get; }
		public string Street { get; }
		public string StreetNumber { get; }
		public string City { get; }

		public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();

		public AgendaOutletViewModel(AppContext context, AgendaOutlet model) : base(model)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			var outlet = model.Outlet;
			this.Number = outlet.Id.ToString();
			this.Name = outlet.Name;
			if (outlet.Addresses.Count > 0)
			{
				var address = outlet.Addresses[0];
				this.Street = address.Street;
				this.StreetNumber = address.Number;
				this.City = address.City;
			}

			foreach (var activity in model.Activities)
			{
				this.Activities.Add(new ActivityViewModel(this, activity));
			}
		}

		public async Task CloseAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			var hasActiveDayBefore = false;
			throw new NotImplementedException();
		}

		public async Task CancelAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			// TODO : !!!
			var cancelReasonSelector = default(ICancelReasonSelector);
			var cancelReason = await cancelReasonSelector.SelectReasonAsync();
			if (cancelReason == null)
			{
				return;
			}

			// TODO : !!!
			var cancelOperation = new CalendarCancelOperation(new CalendarDataProvider(this.Context.DbContextCreator));
			cancelOperation.CancelActivities(new[] { activityViewModel.Model }, cancelReason, a =>
			{
				var aid = a.Id;

				var activities = this.Activities;
				for (var i = 0; i < activities.Count; i++)
				{
					var activity = activities[i];
					if (activity.Model.Id == aid)
					{
						activities[i] = new ActivityViewModel(this, a);
						break;
					}
				}
			});
		}

		public void Copy(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		public void Move(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		public void Delete(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		public void Execute(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}
	}

	public sealed class ActivityViewModel : ViewModel<Activity>
	{
		public AgendaOutletViewModel OutletViewModel { get; }

		public DateTime Date { get; }
		public string Type { get; }
		public string Status { get; }
		public string Details { get; }

		public ICommand CloseCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand MoveCommand { get; }
		public ICommand CopyCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand ExecuteCommand { get; }

		public ActivityViewModel(AgendaOutletViewModel outletViewModel, Activity model) : base(model)
		{
			if (outletViewModel == null) throw new ArgumentNullException(nameof(outletViewModel));
			if (model == null) throw new ArgumentNullException(nameof(model));

			this.OutletViewModel = outletViewModel;
			this.Date = model.Date;
			this.Type = model.Type.Name;
			this.Status = model.Status.Name;
			this.Details = model.Details;

			this.CloseCommand = new RelayCommand(async () =>
			{
				try
				{
					await this.OutletViewModel.CloseAsync(this);
				}
				catch (Exception ex)
				{
					this.OutletViewModel.Context.Log(ex.ToString(), LogLevel.Error);
				}
			});
			this.CancelCommand = new RelayCommand(async () =>
			{
				try
				{
					await this.OutletViewModel.CancelAsync(this);
				}
				catch (Exception ex)
				{
					this.OutletViewModel.Context.Log(ex.ToString(), LogLevel.Error);
				}
			});
			this.MoveCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Move(this);
			});
			this.CopyCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Copy(this);
			});
			this.DeleteCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Delete(this);
			});
			this.ExecuteCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Execute(this);
			});
		}
	}




	public sealed class AgendaDataProvider
	{
		public List<AgendaOutlet> GetAgendaOutlets(IDbContext context, DataCache cache, User user, DateTime date)
		{
			var outlets = new List<AgendaOutlet>();

			var visits = GetVisits(context, cache, user, date);

			//visits.Sort();

			foreach (var byOutlet in visits.GroupBy(v => v.Outlet))
			{
				var activities = byOutlet.SelectMany(v => v.Activities).ToList();

				Sort(activities);

				outlets.Add(new AgendaOutlet(byOutlet.Key, activities));
			}

			return outlets;
		}

		private List<Visit> GetVisits(IDbContext context, DataCache cache, User user, DateTime date)
		{
			// TODO : Query the database Visit, Activities
			// Get outlets from the cache

			// What about filtering to only the active ones ???
			throw new NotImplementedException();
		}

		private static void Sort(List<Activity> activities)
		{
			activities.Sort((x, y) =>
			{
				var cmp = string.Compare(x.Type.Name, y.Type.Name, StringComparison.OrdinalIgnoreCase);

				if (cmp == 0)
				{
					cmp = x.Id.CompareTo(y.Id);
				}

				return cmp;
			});
		}
	}



	public interface ICancelReasonSelector
	{
		Task<CancelReason> SelectReasonAsync();
	}


	public sealed class CancelReason
	{
		public long Id { get; }
		public string Name { get; }

		public CancelReason(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}


	public interface ILocalizationManager
	{
		string GetBy(string context, string key);
	}

	public sealed class CalendarViewModel : ViewModel
	{
		private DateTime CurrentMonth { get; set; }

		private Agenda Agenda { get; }
		private IModalDialog ModalDialog { get; }
		private IAppNavigator AppNavigator { get; }
		private ILocalizationManager LocalizationManager { get; }
		private ICalendarDataProvider DataProvider { get; }
		private ICancelReasonSelector CancelReasonSelector { get; }
		private DataCache DataCache { get; }

		public ObservableCollection<CalendarDayViewModel> Days { get; } = new ObservableCollection<CalendarDayViewModel>();
		public ObservableCollection<CalendarDayViewModel> SelectedDays { get; } = new ObservableCollection<CalendarDayViewModel>();

		public ICommand CloseDaysCommand { get; }
		public ICommand CancelDaysCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand PreviousDayCommand { get; }

		public CalendarViewModel(Agenda agenda, IModalDialog modalDialog, ICancelReasonSelector cancelReasonSelector, IAppNavigator appNavigator, ICalendarDataProvider dataProvider, ILocalizationManager localizationManager, DataCache dataCache)
		{
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));
			if (cancelReasonSelector == null) throw new ArgumentNullException(nameof(cancelReasonSelector));

			this.Agenda = agenda;
			this.ModalDialog = modalDialog;
			this.CancelReasonSelector = cancelReasonSelector;
			this.AppNavigator = appNavigator;
			this.DataProvider = dataProvider;
			this.LocalizationManager = localizationManager;
			this.DataCache = dataCache;
			this.CloseDaysCommand = new RelayCommand(this.CloseSelectedDays);
			this.CancelDaysCommand = new RelayCommand(this.CancelSelectedDays);
			this.CurrentMonth = DateTime.Today;
			this.NextDayCommand = new RelayCommand(() =>
			{
				this.Load(this.CurrentMonth.AddMonths(1));
			});
			this.PreviousDayCommand = new RelayCommand(() =>
			{
				this.Load(this.CurrentMonth.AddMonths(-1));
			});
		}

		public void Load(DateTime date)
		{
			this.CurrentMonth = date;

			foreach (var day in this.DataProvider.GetCalendar(this.Agenda.User, this.CurrentMonth))
			{
				this.Days.Add(new CalendarDayViewModel(day, this));
			}
		}

		public void ViewAgenda(CalendarDayViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			// TODO : How to navigate to a different screen
			//this.AppNavigator.NavigateTo(null, viewModel.Date);
		}

		public async void CancelDays(ICollection<CalendarDayViewModel> viewModels)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));

			var confirmationMessage = this.LocalizationManager.GetBy(@"Calendar", @"ConfirmCancelDay");
			var isConfirmed = (await this.ModalDialog.ShowAsync(confirmationMessage, Feature.None, DialogType.AcceptDecline)) == DialogResult.Accept;
			if (!isConfirmed) return;

			// We don't have any selected days
			if (viewModels.Count == 0) return;

			CancelReason cancelReason = null;

			var operation = new CalendarCancelOperation(this.DataProvider);

			var daysChecker = new CalendarDaysChecker(this.DataProvider.GetPreviousDays);

			foreach (var day in GetDays(viewModels))
			{
				var activeDay = daysChecker.GetActiveDayBefore(GetCurrentDays(this.Days), day);

				var hasActiveDayBefore = activeDay != null;
				if (hasActiveDayBefore)
				{
					var message = this.LocalizationManager.GetBy(@"Calendar", @"ActiveDayBefore") + activeDay.Date.ToString(@"D");
					await this.ModalDialog.ShowAsync(message, Feature.None);
					break;
				}

				if (cancelReason == null)
				{
					// Load all visit/activities ??? => no
					// TODO :
					var activities = this.GetCurrentDayActivities();
					var hasActivitiesForCancel = activities.Any();
					if (hasActivitiesForCancel)
					{

					}
					if (hasActivitiesForCancel || this.DataProvider.HasActivitiesForCancel(day))
					{
						// Prompt for a cancel cancelReason selection	
						cancelReason = await this.CancelReasonSelector.SelectReasonAsync();

						// Cancel the operation
						if (cancelReason == null) return;
					}
				}

				// Execute Cancel day
				operation.CancelDays(this.DataCache, day, cancelReason, this.UpdateDayStatus);
			}
		}

		private IEnumerable<Activity> GetCurrentDayActivities()
		{
			throw new NotImplementedException();
		}


		private static IEnumerable<CalendarDay> GetCurrentDays(IReadOnlyList<CalendarDayViewModel> days)
		{
			for (var i = days.Count - 1; i >= 0; i--)
			{
				yield return days[i].Model;
			}
		}

		private void CloseSelectedDays()
		{
			throw new NotImplementedException();
		}

		private void CancelSelectedDays()
		{
			this.CancelDays(this.SelectedDays);
		}

		private void UpdateDayStatus(CalendarDay day)
		{
			for (var i = 0; i < this.Days.Count; i++)
			{
				var viewModel = this.Days[i];
				if (viewModel.Model.Date == day.Date)
				{
					this.Days[i] = new CalendarDayViewModel(new CalendarDay(day.Date, day.Status), this);
					break;
				}
			}
		}

		private static CalendarDay[] GetDays(ICollection<CalendarDayViewModel> viewModels)
		{
			var days = new CalendarDay[viewModels.Count];

			var index = 0;
			foreach (var viewModel in viewModels)
			{
				days[index++] = viewModel.Model;
			}

			Array.Sort(days, (x, y) => x.Date.CompareTo(y.Date));

			return days;
		}
	}

	public sealed class CalendarDay
	{
		public DateTime Date { get; }
		public DayStatus Status { get; }

		public CalendarDay(DateTime date, DayStatus status)
		{
			if (status == null) throw new ArgumentNullException(nameof(status));

			this.Date = date;
			this.Status = status;
		}
	}

	public sealed class DayStatus
	{
		public long Id { get; }
		public string Name { get; }
		public bool IsOpen => this.Id == 0;
		public bool IsWorking => this.Id == 1;
		public bool IsCancel => this.Id == 2;
		public bool IsClose => this.Id == 3;
		public bool IsActive => this.IsOpen || this.IsWorking;

		public DayStatus(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class CalendarDaysChecker
	{
		private List<CalendarDay> _previousDays = default(List<CalendarDay>);

		public Func<CalendarDay, List<CalendarDay>> PreviousDaysProvider { get; }

		public CalendarDaysChecker(Func<CalendarDay, List<CalendarDay>> previousDaysProvider)
		{
			if (previousDaysProvider == null) throw new ArgumentNullException(nameof(previousDaysProvider));

			this.PreviousDaysProvider = previousDaysProvider;
		}

		public CalendarDay GetActiveDayBefore(IEnumerable<CalendarDay> days, CalendarDay day)
		{
			if (days == null) throw new ArgumentNullException(nameof(days));
			if (day == null) throw new ArgumentNullException(nameof(day));

			// Search current days
			var activeDay = GetActiveDay(days, day);
			if (activeDay != null) return activeDay;

			// Lazy Load previous days
			if (_previousDays == null)
			{
				_previousDays = this.PreviousDaysProvider(day);
			}

			// Search previous days
			return GetActiveDay(_previousDays, day);
		}

		private static CalendarDay GetActiveDay(IEnumerable<CalendarDay> days, CalendarDay day)
		{
			var date = day.Date;

			foreach (var current in days)
			{
				if (current.Date >= date)
				{
					// Ignore days in the future
					continue;
				}
				if (current.Status.IsActive)
				{
					return current;
				}
			}

			return null;
		}
	}

	public static class CalendarHelper
	{
		public static CalendarViewModel GetCalendarViewModel(DataCache cache, User user, DateTime date)
		{
			return new CalendarViewModel(null, null, null, null, null, null, cache);
		}
	}

	public sealed class CalendarCancelOperation
	{
		private ICalendarDataProvider DataProvider { get; }

		public CalendarCancelOperation(ICalendarDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void CancelDays(DataCache cache, CalendarDay day, CancelReason cancelReason, Action<CalendarDay> dayCancelled)
		{
			if (day == null) throw new ArgumentNullException(nameof(day));
			if (cancelReason == null) throw new ArgumentNullException(nameof(cancelReason));

			using (var dbContext = this.DataProvider.DbContextCreator())
			{
				// We can cancel only open days
				if (day.Status.IsOpen)
				{
					this.CancelActivities(dbContext, day, cancelReason);

					var cancelStatus = cache.GetValues<DayStatus>(dbContext).Values.Single(s => s.IsCancel);

					// Mark date as cancelled
					this.DataProvider.Cancel(dbContext, day, cancelStatus);

					// Fire the "event"
					dayCancelled.Invoke(new CalendarDay(day.Date, cancelStatus));
				}

				dbContext.Complete();
			}
		}

		public void CancelActivities(IDbContext dbContext, CalendarDay day, CancelReason cancelReason)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (day == null) throw new ArgumentNullException(nameof(day));
			if (cancelReason == null) throw new ArgumentNullException(nameof(cancelReason));

			// TODO : !!!
			// Cancel all open activities
			//this.DataProvider.CancelActivities(dbContext, null, cancelReason);
			//throw new NotImplementedException();
		}

		public void CancelActivities(ICollection<Activity> activities, CancelReason cancelReason, Action<Activity> activityCancelled)
		{
			if (activities == null) throw new ArgumentNullException(nameof(activities));
			if (cancelReason == null) throw new ArgumentNullException(nameof(cancelReason));

			// TODO : !!!
			// Cancel all open activities
			//this.DataProvider.CancelActivities(dbContext, null, cancelReason);
			//throw new NotImplementedException();
		}



	}

	public sealed class CalendarDayViewModel : ViewModel<CalendarDay>
	{
		private CalendarViewModel ViewModel { get; }
		public DateTime Date { get; }
		public DayStatus Status { get; }

		public ICommand CloseDayCommand { get; }
		public ICommand CancelDayCommand { get; }
		public ICommand ViewAgendaCommand { get; }

		public CalendarDayViewModel(CalendarDay model, CalendarViewModel viewModel) : base(model)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.ViewModel = viewModel;
			this.Date = model.Date;
			this.Status = model.Status;
			this.CloseDayCommand = new RelayCommand(() =>
			{
				throw new NotImplementedException();
			});
			this.CancelDayCommand = new RelayCommand(() =>
			{
				this.ViewModel.CancelDays(new[] { this });
			});
			this.ViewAgendaCommand = new RelayCommand(() =>
			{
				this.ViewModel.ViewAgenda(this);
			});
		}
	}

	public interface ICalendarDataProvider
	{
		Func<IDbContext> DbContextCreator { get; }

		ICollection<CalendarDay> GetCalendar(User user, DateTime date);

		List<CalendarDay> GetPreviousDays(CalendarDay day);

		bool HasActivitiesForCancel(CalendarDay day);

		void CancelActivities(IDbContext context, ICollection<Activity> activities, CancelReason cancelReason);

		void Cancel(IDbContext context, CalendarDay day, DayStatus cancelStatus);
	}

	public sealed class CalendarDataProvider : ICalendarDataProvider
	{
		public Func<IDbContext> DbContextCreator { get; }

		public CalendarDataProvider(Func<IDbContext> dbContextCreator)
		{
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));

			this.DbContextCreator = dbContextCreator;
		}

		public ICollection<CalendarDay> GetCalendar(User user, DateTime date)
		{
			// TODO : Query the database => visit days
			//throw new NotImplementedException();
			return null;
		}

		public List<CalendarDay> GetPreviousDays(CalendarDay day)
		{
			throw new NotImplementedException();
		}

		public bool HasActivitiesForCancel(CalendarDay day)
		{
			// Regular activities or Long term activities expiring today
			throw new NotImplementedException();
		}

		public void CancelActivities(IDbContext context, CalendarDay day)
		{
			throw new NotImplementedException();
		}

		public void CancelActivities(IDbContext context, ICollection<Activity> activities, CancelReason cancelReason)
		{
			throw new NotImplementedException();
		}

		public void Cancel(IDbContext context, CalendarDay day, DayStatus cancelStatus)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (day == null) throw new ArgumentNullException(nameof(day));
			if (cancelStatus == null) throw new ArgumentNullException(nameof(cancelStatus));

			var q = @"update calendar set status = @status where date = @date";

			var date = day.Date;
			var status = cancelStatus.Id;

			throw new NotImplementedException();
		}

		public void Close(IDbContext context, List<Activity> activities)
		{
			throw new NotImplementedException();
		}

		public void Close(IDbContext context, CalendarDay day)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AgendaOutlet
	{
		public Outlet Outlet { get; }
		public List<Activity> Activities { get; }

		public AgendaOutlet(Outlet outlet, List<Activity> activities)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			if (activities == null) throw new ArgumentNullException(nameof(activities));

			this.Outlet = outlet;
			this.Activities = activities;
		}
	}


	public static class DataProvider
	{
		public static Dictionary<long, List<ActivityCloseReason>> GetActivityCloseReasons(IDbContext context, DataCache cache)
		{
			var closeReasons = new Dictionary<long, List<ActivityCloseReason>>();

			context.Fill(closeReasons, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				var typeId = r.GetInt64(2);

				List<ActivityCloseReason> local;
				if (!map.TryGetValue(typeId, out local))
				{
					local = new List<ActivityCloseReason>();
					map.Add(typeId, local);
				}
				local.Add(new ActivityCloseReason(id, name));
			}, new Query(@"SELECT ID, NAME, ACTIVITY_TYPE_ID FROM ACTIVITY_CLOSE_REASONS"));

			return closeReasons;
		}

		public static Dictionary<long, List<ActivityCancelReason>> GetActivityCancelReasons(IDbContext context, DataCache cache)
		{
			var cancelReasons = new Dictionary<long, List<ActivityCancelReason>>();

			context.Fill(cancelReasons, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				var typeId = r.GetInt64(2);

				List<ActivityCancelReason> local;
				if (!map.TryGetValue(typeId, out local))
				{
					local = new List<ActivityCancelReason>();
					map.Add(typeId, local);
				}
				local.Add(new ActivityCancelReason(id, name));
			}, new Query(@"SELECT ID, NAME, ACTIVITY_TYPE_ID FROM ACTIVITY_CANCEL_REASONS"));

			return cancelReasons;
		}

		public static Dictionary<long, ActivityType> GetActivityTypes(IDbContext context, DataCache cache)
		{
			var categories = cache.GetValues<ActivityTypeCategory>(context);
			var byTypeCloseReasons = cache.GetValues<List<ActivityCloseReason>>(context);
			var byTypeCancelReasons = cache.GetValues<List<ActivityCancelReason>>(context);

			var types = new Dictionary<long, ActivityType>();

			context.Fill(types, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				var categoryId = r.GetInt64(2);

				var type = new ActivityType(id, name);
				var category = categories[categoryId];

				// Set Auto Selected ActivityType to the current if it's matching
				if (category.AutoSelectedActivityType != null && category.AutoSelectedActivityType.Id == id)
				{
					category.AutoSelectedActivityType = type;
				}

				// Add to related category
				category.Types.Add(type);

				List<ActivityCloseReason> closeReasonsByType;
				if (byTypeCloseReasons.TryGetValue(id, out closeReasonsByType))
				{
					type.CloseReasons.AddRange(closeReasonsByType);
				}

				List<ActivityCancelReason> cancelReasonsByType;
				if (byTypeCancelReasons.TryGetValue(id, out cancelReasonsByType))
				{
					type.CancelReasons.AddRange(cancelReasonsByType);
				}

				map.Add(id, type);
			}, new Query(@"SELECT ID, NAME, ACTIVITY_TYPE_CATEGORY_ID FROM ACTIVITY_TYPES"));

			// We don't need this rows anymore
			cache.RemoveValues<List<ActivityCloseReason>>();
			cache.RemoveValues<List<ActivityCancelReason>>();

			return types;
		}

		public static Dictionary<long, ActivityStatus> GetActivityStatuses(IDbContext context, DataCache cache)
		{
			var statuses = new Dictionary<long, ActivityStatus>();

			context.Fill(statuses, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);

				map.Add(id, new ActivityStatus(id, name));
			}, new Query(@"SELECT ID, NAME FROM ACTIVITY_STATUSES"));

			return statuses;
		}

		public static Dictionary<long, ActivityTypeCategory> GetActivityTypeCategories(IDbContext context, DataCache cache)
		{
			var categories = new Dictionary<long, ActivityTypeCategory>();

			context.Fill(categories, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);

				var category = new ActivityTypeCategory(id, name);
				if (!r.IsDbNull(2))
				{
					category.AutoSelectedActivityType = new ActivityType(r.GetInt64(2), string.Empty);
				}

				map.Add(id, category);
			}, new Query(@"SELECT ID, NAME, AUTO_SELECTED_ACTIVITY_TYPE_ID FROM ACTIVITY_TYPE_CATEGORIES"));

			return categories;
		}

		public static Dictionary<long, Outlet> GetOutlets(IDbContext context, DataCache cache)
		{
			var outlets = new Dictionary<long, Outlet>();

			context.Fill(outlets, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);

				var outlet = new Outlet(id, name);

				map.Add(id, outlet);
			}, new Query(@"SELECT ID, NAME, AUTO_SELECTED_ACTIVITY_TYPE_ID FROM ACTIVITY_TYPE_CATEGORIES"));


			var addresses = cache.GetValues<List<OutletAddress>>(context);
			foreach (var outlet in outlets.Values)
			{
				List<OutletAddress> byOutlet;
				if (addresses.TryGetValue(outlet.Id, out byOutlet))
				{
					outlet.Addresses.AddRange(byOutlet);
				}
			}

			// We don't need them anymore as they are assigned to the respective outlets
			cache.RemoveValues<List<OutletAddress>>();



			return outlets;
		}

		public static Dictionary<long, List<OutletAddress>> GetOutletAddressed(IDbContext context, DataCache cache)
		{
			var addresses = new Dictionary<long, List<OutletAddress>>();

			context.Fill(addresses, (r, map) =>
			{
				List<OutletAddress> byOutlet;
				if (!map.TryGetValue(-1, out byOutlet))
				{
					byOutlet = new List<OutletAddress>();
					map.Add(-1, byOutlet);
				}

				byOutlet.Add(new OutletAddress(-1, 0, string.Empty, string.Empty, string.Empty));
			}, new Query(@"SELECT ID, NAME, AUTO_SELECTED_ACTIVITY_TYPE_ID FROM ACTIVITY_TYPE_CATEGORIES"));

			return addresses;
		}
	}

	public sealed class ActivityType
	{
		public long Id { get; }
		public string Name { get; }
		public List<ActivityCloseReason> CloseReasons { get; } = new List<ActivityCloseReason>();
		public List<ActivityCancelReason> CancelReasons { get; } = new List<ActivityCancelReason>();

		public ActivityType(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class ActivityTypeCategory
	{
		public long Id { get; }
		public string Name { get; }
		public List<ActivityType> Types { get; } = new List<ActivityType>();
		public ActivityType AutoSelectedActivityType { get; set; }

		public ActivityTypeCategory(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class Visit
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }
		public List<Activity> Activities { get; } = new List<Activity>();

		public Visit(long id, Outlet outlet, DateTime fromDate, DateTime toDate)
		{
			Id = id;
			Outlet = outlet;
			FromDate = fromDate;
			ToDate = toDate;
		}
	}

	public sealed class Activity
	{
		public long Id { get; set; }
		public ActivityType Type { get; }
		public ActivityStatus Status { get; }
		public DateTime Date { get; }
		public string Details { get; }
	}

	public sealed class ActivityStatus
	{
		public long Id { get; }
		public string Name { get; }
		public bool IsActive => this.Id == 0 || this.IsWorking;
		public bool IsWorking => this.Id == 1;

		public ActivityStatus(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class ActivityCloseReason
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityCloseReason(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class ActivityCancelReason
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityCancelReason(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}