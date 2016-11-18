using System;
using System.CodeDom;
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
using Cchbc.Objects;

namespace ConsoleClient
{
	public sealed class AppModule
	{
		public AppContext AppContext { get; }

		public AppModule(AppContext appContext)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));

			this.AppContext = appContext;
		}

		public void Init()
		{
			var cache = this.AppContext.DataCache;
			cache.Register(DataProvider.GetActivityCancelReasons);
			cache.Register(DataProvider.GetActivityCloseReasons);
			cache.Register(DataProvider.GetActivityTypes);
			cache.Register(DataProvider.GetActivityTypeCategories);
			cache.Register(DataProvider.GetActivityStatuses);
		}

		public void Load()
		{
			using (var ctx = this.AppContext.DbContextCreator())
			{
				var cache = this.AppContext.DataCache;

				var s = Stopwatch.StartNew();
				var types = cache.GetValues<ActivityType>(ctx).Values;
				s.Stop();
				Console.WriteLine(s.ElapsedMilliseconds);
				foreach (var type in types)
				{
					Console.WriteLine(type.Id + @" " + type.Name);
					Console.WriteLine(@"Close reasons  " + type.CloseReasons.Count);
					Console.WriteLine(@"Cancel reasons " + type.CancelReasons.Count);
				}

				var outlet = new Outlet(1, @"Billa");



				ctx.Complete();
			}
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
			cache.GetValues<TradeChannel>(context).TryGetValue(outlet.TradeChannelId, out result);

			return result ?? TradeChannel.Empty;
		}

		public static SubTradeChannel GetSubTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			SubTradeChannel result;
			cache.GetValues<SubTradeChannel>(context).TryGetValue(outlet.SubTradeChannelId, out result);

			return result ?? SubTradeChannel.Empty;
		}
	}


	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }
		public List<OutletAddress> Addresses { get; } = new List<OutletAddress>();
		public long TradeChannelId { get; }
		public long SubTradeChannelId { get; }
		public long DeliveryLocationId { get; }

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


	public sealed class Agenda
	{
		public DateTime CurrentDate { get; }
		public List<AgendaOutlet> Outlets { get; }

		public Agenda(DateTime currentDate, List<AgendaOutlet> outlets)
		{
			CurrentDate = currentDate;
			Outlets = outlets;
		}
	}

	public class AgendaHelper
	{
		public IAgendaDataProvider DataProvider { get; }

		public AgendaHelper(IAgendaDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			DataProvider = dataProvider;
		}



		public Agenda GetAgenda(User user, DateTime date)
		{
			var outlets = this.DataProvider.GetAgendaOutlets(null, null, user, date);
			return new Agenda(date, outlets);
		}
	}

	public interface IAgendaDataProvider
	{
		List<AgendaOutlet> GetAgendaOutlets(IDbContext context, DataCache cache, User user, DateTime date);
	}

	public sealed class AgendaDataProvider : IAgendaDataProvider
	{
		public List<AgendaOutlet> GetAgendaOutlets(IDbContext context, DataCache cache, User user, DateTime date)
		{
			var outlets = new List<AgendaOutlet>();

			foreach (var byOutlet in GetVisits(context, cache, user, date).GroupBy(v => v.Outlet))
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

		public static void Sort(List<Activity> activities)
		{
			if (activities == null) throw new ArgumentNullException(nameof(activities));

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

	public static class CalendarHelper
	{
		public static CalendarViewModel GetCalendarViewModel(IDbContext context, DataCache cache, User user, DateTime date)
		{
			return new CalendarViewModel(null, null, null, null, null, default(ICalendarDataProvider));
		}
	}

	public interface IModalDialog
	{
		Task<bool> RequestConfirmationAsync(string message);
	}

	public interface ICancelReasonSelector
	{
		Task<DayCancelReason> SelectReasonAsync();
	}

	public interface ICloseReasonSelector
	{
		Task<DayCloseReason> SelectReasonAsync();
	}

	public sealed class DayCancelReason
	{
		public long Id { get; }
		public string Name { get; }

		public DayCancelReason(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class DayCloseReason
	{
		public long Id { get; }
		public string Name { get; }

		public DayCloseReason(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class CalendarViewModel : ViewModel
	{
		private DateTime CurrentMonth { get; set; }

		private User User { get; }
		private IModalDialog ModalDialog { get; }
		private IAppNavigator AppNavigator { get; }
		private ICalendarDataProvider DataProvider { get; }
		private ICancelReasonSelector CancelReasonSelector { get; }
		private ICloseReasonSelector CloseReasonSelector { get; }

		public ObservableCollection<CalendarDayViewModel> Days { get; } = new ObservableCollection<CalendarDayViewModel>();
		public ObservableCollection<CalendarDayViewModel> SelectedDays { get; } = new ObservableCollection<CalendarDayViewModel>();

		public ICommand CloseDaysCommand { get; }
		public ICommand CancelDaysCommand { get; }
		public ICommand NextDayCommand { get; }
		public ICommand PreviousDayCommand { get; }

		public CalendarViewModel(User user, IModalDialog modalDialog, ICancelReasonSelector cancelReasonSelector, ICloseReasonSelector closeReasonSelector, IAppNavigator appNavigator, ICalendarDataProvider dataProvider)
		{
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));
			if (cancelReasonSelector == null) throw new ArgumentNullException(nameof(cancelReasonSelector));
			if (closeReasonSelector == null) throw new ArgumentNullException(nameof(closeReasonSelector));

			this.User = user;
			this.ModalDialog = modalDialog;
			this.CancelReasonSelector = cancelReasonSelector;
			this.CloseReasonSelector = closeReasonSelector;
			this.AppNavigator = appNavigator;
			this.DataProvider = dataProvider;
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

		public void Load(DateTime? date)
		{
			this.CurrentMonth = date ?? this.CurrentMonth;

			foreach (var day in this.DataProvider.GetCalendar(this.User, this.CurrentMonth))
			{
				this.Days.Add(new CalendarDayViewModel(day, this));
			}
		}

		public void ViewAgenda(CalendarDayViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			// TODO : How to navigate to a different screen
			this.AppNavigator.NavigateTo(null, viewModel.Date);
		}

		public async void CancelDays(ICollection<CalendarDayViewModel> days)
		{
			if (days == null) throw new ArgumentNullException(nameof(days));

			var isConfirmed = await this.ModalDialog.RequestConfirmationAsync(@"Are you sure you want to cancel day?");
			if (!isConfirmed) return;

			// We don't have selected days
			if (days.Count == 0) return;

			// Prompt for a cancel cancelReason selection
			var cancelReason = await this.CancelReasonSelector.SelectReasonAsync();
			if (cancelReason == null) return;

			// Setup the update
			var operation = new CalendarCancelOperation(this.UpdateDayStatus);

			// Execute Cancel day
			operation.CancelDays(days, cancelReason);
		}

		public async void CloseDays(ICollection<CalendarDayViewModel> days)
		{
			if (days == null) throw new ArgumentNullException(nameof(days));

			var isConfirmed = await this.ModalDialog.RequestConfirmationAsync(@"Are you sure you want to close day?");
			if (!isConfirmed) return;

			// We don't have selected days			
			if (days.Count == 0) return;

			// Prompt for a cancel cancelReason selection
			var closeReason = await this.CloseReasonSelector.SelectReasonAsync();
			if (closeReason == null) return;

			// Setup the update
			var operation = new CalendarCloseOperation(this.UpdateDayStatus);

			// Execute Close day
			operation.CloseDays(days, closeReason);
		}

		private void CancelSelectedDays()
		{
			this.CancelDays(this.SelectedDays);
		}

		private void CloseSelectedDays()
		{
			CloseDays(this.SelectedDays);
		}

		private void UpdateDayStatus(CalendarDayViewModel day, DayStatus status)
		{
			foreach (var viewModel in this.Days)
			{
				if (viewModel == day)
				{
					viewModel.Status = status;
					break;
				}
			}
		}

	}

	public sealed class CalendarCancelOperation
	{
		public Action<CalendarDayViewModel, DayStatus> DayCancelled { get; }

		public CalendarCancelOperation(Action<CalendarDayViewModel, DayStatus> dayCancelled)
		{
			if (dayCancelled == null) throw new ArgumentNullException(nameof(dayCancelled));

			this.DayCancelled = dayCancelled;
		}

		public void CancelDays(ICollection<CalendarDayViewModel> days, DayCancelReason cancelReason)
		{
			if (days == null) throw new ArgumentNullException(nameof(days));
			if (cancelReason == null) throw new ArgumentNullException(nameof(cancelReason));

			// TODO : !!!
			var cancelStatus = new DayStatus(1, @"Cancel Status");

			foreach (var model in days)
			{
				var canCancel = this.CanCancel(model);
				if (canCancel)
				{
					// Cancel the day with the specified reason
					this.Cancel(model, cancelReason);

					// Fire the "event"					
					this.DayCancelled?.Invoke(model, cancelStatus);
				}
			}
		}

		private bool CanCancel(CalendarDayViewModel model)
		{
			// Load some
			// Perform the check for can cancel
			// TODO : 
			throw new NotImplementedException();
		}

		private void Cancel(CalendarDayViewModel model, DayCancelReason cancelReason)
		{
			// Perform the update
			// TODO : 
			throw new NotImplementedException();
		}
	}

	public sealed class CalendarCloseOperation
	{
		public Action<CalendarDayViewModel, DayStatus> DayClosed { get; }

		public CalendarCloseOperation(Action<CalendarDayViewModel, DayStatus> dayClosed)
		{
			if (dayClosed == null) throw new ArgumentNullException(nameof(dayClosed));

			this.DayClosed = dayClosed;
		}

		public void CloseDays(ICollection<CalendarDayViewModel> days, DayCloseReason closeReason)
		{
			if (days == null) throw new ArgumentNullException(nameof(days));
			if (closeReason == null) throw new ArgumentNullException(nameof(closeReason));

			// TODO : !!!
			var closeStatus = new DayStatus(1, @"Close Status");

			foreach (var model in days)
			{
				var canClose = this.CanClose(model);
				if (canClose)
				{
					// Close the day with the specified reason
					this.Close(model, closeReason);

					// Fire the "event"					
					this.DayClosed?.Invoke(model, closeStatus);
				}
			}
		}

		private bool CanClose(CalendarDayViewModel model)
		{
			// Load some
			// Perform the check for can cancel
			// TODO : 
			throw new NotImplementedException();
		}

		private void Close(CalendarDayViewModel model, DayCloseReason closeReason)
		{
			// Perform the update
			// TODO : 
			throw new NotImplementedException();
		}
	}

	public interface IAppNavigator
	{
		void NavigateTo(object tmp, object state);
	}

	public sealed class CalendarDayViewModel : ViewModel<CalendarDay>
	{
		private CalendarViewModel ViewModel { get; }
		public DateTime Date { get; }

		private DayStatus _status;
		public DayStatus Status
		{
			get { return _status; }
			set { this.SetProperty(out _status, value); }
		}

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
				this.ViewModel.CloseDays(new[] { this });
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
		ICollection<CalendarDay> GetCalendar(User user, DateTime date);
	}

	public sealed class CalendarDataProvider : ICalendarDataProvider
	{
		public ICollection<CalendarDay> GetCalendar(User user, DateTime date)
		{
			// TODO : Query the database => visit days
			//throw new NotImplementedException();
			return null;
		}
	}

	public sealed class CalendarDay
	{
		public DateTime Date { get; }
		public DayStatus Status { get; }

		public CalendarDay(DateTime date, DayStatus status)
		{
			Date = date;
			Status = status;
		}
	}

	public sealed class DayStatus
	{
		public long Id { get; }
		public string Name { get; }

		public DayStatus(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class User
	{
		public long Id { get; }
		public string Name { get; }

		public User(long id, string name)
		{
			Id = id;
			Name = name;
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
		public ActivityStatus Status { get; set; }
	}

	public sealed class ActivityStatus
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityStatus(long id, string name)
		{
			Id = id;
			Name = name;
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