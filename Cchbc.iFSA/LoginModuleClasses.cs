using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Logs;
using Cchbc.Validation;
using iFSA.Common.Objects;

namespace iFSA
{
	//public sealed class LoginScreen
	//{
	//	public LoginScreenViewModel ViewModel { get; } = new LoginScreenViewModel(GlobalAppContext.MainContext, GlobalAppContext.AppNavigator, null);

	//	public async void LoadData()
	//	{
	//		try
	//		{
	//			await this.ViewModel.LoadDataAsync();
	//		}
	//		catch (Exception ex)
	//		{
	//			GlobalAppContext.MainContext.Log(ex.ToString(), LogLevel.Error);
	//		}
	//	}
	//}

	//public sealed class AgendaScreen
	//{
	//	public AgendaScreenViewModel ViewModel { get; } = new AgendaScreenViewModel(GlobalAppContext.MainContext, GlobalAppContext.Agenda, GlobalAppContext.AppNavigator, GlobalAppContext.UIThreadDispatcher);

	//	public void LoadData()
	//	{
	//		try
	//		{
	//			// Get from parameter
	//			var outlets = default(List<AgendaOutlet>);
	//			this.ViewModel.LoadData(outlets);
	//		}
	//		catch (Exception ex)
	//		{
	//			GlobalAppContext.MainContext.Log(ex.ToString(), LogLevel.Error);
	//		}
	//	}
	//}


	public sealed class Calendar
	{

	}


	public sealed class CancelReason
	{
		public long Id { get; }
		public string Name { get; }

		public CancelReason(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class CalendarViewModel : ViewModel
	{
		private DateTime CurrentMonth { get; set; }

		private User User { get; }
		private IModalDialog ModalDialog { get; }
		private IAppNavigator AppNavigator { get; }
		private Func<string, string, string> LocalizationManager { get; }
		private ICalendarDataProvider DataProvider { get; }
		private Func<Task<CancelReason>> CancelReasonSelector { get; }
		private DataCache DataCache { get; }

		public ObservableCollection<CalendarDayViewModel> Days { get; } = new ObservableCollection<CalendarDayViewModel>();
		public ObservableCollection<CalendarDayViewModel> SelectedDays { get; } = new ObservableCollection<CalendarDayViewModel>();

		public ICommand CloseDaysCommand { get; }
		public ICommand CancelDaysCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand PreviousDayCommand { get; }

		public CalendarViewModel(IModalDialog modalDialog, Func<Task<CancelReason>> cancelReasonSelector, IAppNavigator appNavigator, ICalendarDataProvider dataProvider, Func<string, string, string> localizationManager, DataCache dataCache)
		{
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));
			if (cancelReasonSelector == null) throw new ArgumentNullException(nameof(cancelReasonSelector));


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

			//foreach (var day in this.DataProvider.GetCalendar(this.Agenda.User, this.CurrentMonth))
			//{
			//	this.Days.Add(new CalendarDayViewModel(day, this));
			//}
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

			var confirmationMessage = this.LocalizationManager(@"Calendar", @"ConfirmCancelDay");
			var isConfirmed = (await this.ModalDialog.ShowAsync(confirmationMessage, Feature.None, PermissionType.Confirm)) == DialogResult.Accept;
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
					var message = this.LocalizationManager(@"Calendar", @"ActiveDayBefore") + activeDay.Date.ToString(@"D");
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
						cancelReason = await this.CancelReasonSelector();

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
			return new CalendarViewModel(null, null, null, null, null, cache);
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









}