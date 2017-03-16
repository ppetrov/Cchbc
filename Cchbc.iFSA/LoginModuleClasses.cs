﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Logs;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;
using iFSA.LoginModule;

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



	public sealed class AgendaOutletViewModel : ViewModel<AgendaOutlet>
	{
		public MainContext Context { get; }

		public long Number { get; }
		public string Name { get; }
		public string Street { get; }
		public string StreetNumber { get; }
		public string City { get; }

		private string _outletImage;
		public string OutletImage
		{
			get { return _outletImage; }
			set { this.SetProperty(ref _outletImage, value); }
		}

		public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();

		public AgendaOutletViewModel(MainContext context, AgendaOutlet model) : base(model)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			var outlet = model.Outlet;
			this.Number = outlet.Id;
			this.Name = outlet.Name;
			if (outlet.Addresses.Count > 0)
			{
				var address = outlet.Addresses[0];
				this.Street = address.Street;
				this.StreetNumber = address.Number.ToString();
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

	public sealed class Calendar
	{

	}

	public sealed class AgendaScreenViewModel : ViewModel
	{
		private Agenda Agenda { get; }
		private IAppNavigator AppNavigator { get; }
		private IUIThreadDispatcher UIThreadDispatcher { get; }
		private MainContext MainContext { get; }

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

		public AgendaScreenViewModel(MainContext mainContext, Agenda agenda, IAppNavigator appNavigator, IUIThreadDispatcher uiThreadDispatcher)
		{
			if (agenda == null) throw new ArgumentNullException(nameof(agenda));
			if (appNavigator == null) throw new ArgumentNullException(nameof(appNavigator));
			if (uiThreadDispatcher == null) throw new ArgumentNullException(nameof(uiThreadDispatcher));

			this.Agenda = agenda;
			this.AppNavigator = appNavigator;
			this.UIThreadDispatcher = uiThreadDispatcher;
			this.MainContext = mainContext;
			this.PreviousDayCommand = new RelayCommand(() =>
			{
				try
				{
					this.Agenda.LoadPreviousDay(this.MainContext);
					this.SetupData();
				}
				catch (Exception ex)
				{
					this.MainContext.Log(ex.ToString(), LogLevel.Error);
				}
			});
			this.NextDayCommand = new RelayCommand(() =>
			{
				try
				{
					this.Agenda.LoadNextDay(this.MainContext);
					this.SetupData();
				}
				catch (Exception ex)
				{
					this.MainContext.Log(ex.ToString(), LogLevel.Error);
				}
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
			this.Agenda.LoadDay(this.MainContext, user, dateTime);
			var outlets = new List<AgendaOutlet>
			{
				new AgendaOutlet(new Outlet(1, "Billa"), new List<Activity>() ),
				new AgendaOutlet(new Outlet(2, "Metro"), new List<Activity>() ),
			};
			this.SetupData(outlets);
		}

		private void SetupData()
		{
			var outlets = new List<AgendaOutlet>()
			{
				new AgendaOutlet(new Outlet(1, "Billa"), new List<Activity>() ),
				new AgendaOutlet(new Outlet(2, "Metro"), new List<Activity>() ),
			};
			this.SetupData(outlets);
		}

		private void SetupData(IEnumerable<AgendaOutlet> outlets)
		{
			this.Outlets.Clear();
			foreach (var outlet in outlets)
			{
				this.Outlets.Add(new AgendaOutletViewModel(this.MainContext, outlet));
			}

			Task.Run(() =>
			{
				while (!this.Agenda.ImagesLoadedEvent.IsSet)
				{
					OutletImage outletImage;
					while (this.Agenda.OutletImages.TryDequeue(out outletImage))
					{
						var number = outletImage.Outlet;
						Debug.WriteLine(number.ToString());

						foreach (var viewModel in this.Outlets)
						{
							if (viewModel.Number == number)
							{
								this.UIThreadDispatcher.Dispatch(() =>
								{
									Debug.WriteLine(@"Match:" + number);
									viewModel.OutletImage = DateTime.Now.ToString(@"G");
								});
								break;
							}
						}
					}

					Task.Delay(100).Wait();
				}
			});
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
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class CalendarViewModel : ViewModel
	{
		private DateTime CurrentMonth { get; set; }

		private Agenda Agenda { get; }
		private IModalDialog ModalDialog { get; }
		private IAppNavigator AppNavigator { get; }
		private Func<string, string, string> LocalizationManager { get; }
		private ICalendarDataProvider DataProvider { get; }
		private ICancelReasonSelector CancelReasonSelector { get; }
		private DataCache DataCache { get; }

		public ObservableCollection<CalendarDayViewModel> Days { get; } = new ObservableCollection<CalendarDayViewModel>();
		public ObservableCollection<CalendarDayViewModel> SelectedDays { get; } = new ObservableCollection<CalendarDayViewModel>();

		public ICommand CloseDaysCommand { get; }
		public ICommand CancelDaysCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand PreviousDayCommand { get; }

		public CalendarViewModel(Agenda agenda, IModalDialog modalDialog, ICancelReasonSelector cancelReasonSelector, IAppNavigator appNavigator, ICalendarDataProvider dataProvider, Func<string, string, string> localizationManager, DataCache dataCache)
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

			var confirmationMessage = this.LocalizationManager(@"Calendar", @"ConfirmCancelDay");
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









}