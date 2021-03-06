using System;
using Atos.Client;
using Atos.Client.Validation;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule.Objects
{
	public sealed class Agenda
	{
		private MainContext MainContext { get; }

		public event EventHandler<ActivityEventArgs> ActivityInserted;
		public event EventHandler<ActivityEventArgs> ActivityUpdated;
		public event EventHandler<ActivityEventArgs> ActivityDeleted;

		public Agenda(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public PermissionResult CanChangeStartTime(Activity activity, DateTime dateTime)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			if (activity.Details == string.Empty)
			{
				return PermissionResult.Deny(@"CannotChangeDateOfServerActivity");
			}
			if (activity.Details == @"???")
			{
				return PermissionResult.Confirm(@"OutsideOfWorkingHours");
			}
			return PermissionResult.Allow;
		}

		public void ChangeStartTime(Activity activity, DateTime dateTime)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TDOO !!! Update activity

			new ActivityEventArgs(activity);

			throw new NotImplementedException();
		}

		//public PermissionResult CanCreate(Outlet outlet, ActivityType activityType)
		//{
		//	if (outlet == null)
		//	{
		//		return PermissionResult.Deny(@"MissingOutlet");
		//	}
		//	if (activityType == null)
		//	{
		//		return PermissionResult.Deny(@"MissingActivityType");
		//	}
		//	if (outlet.Id > 0 && activityType.Id > 0)
		//	{
		//		return PermissionResult.Confirm(@"Confirm activity type for this outlet?");
		//	}
		//	return PermissionResult.Allow;
		//}

		//public Activity Create(DataQueryContext context, Outlet outlet, ActivityType type, ActivityStatus status, DateTime date)
		//{
		//	if (context == null) throw new ArgumentNullException(nameof(context));
		//	if (type == null) throw new ArgumentNullException(nameof(type));
		//	if (status == null) throw new ArgumentNullException(nameof(status));
		//	if (outlet == null) throw new ArgumentNullException(nameof(outlet));

		//	var activity = new Activity(0, outlet, type, status, date, date, string.Empty);

		//	// Save activity to db
		//	var newActivity = this.DataProvider.Insert(context, activity);

		//	// Find item by outlet
		//	var agendaOutlet = default(AgendaOutlet);
		//	foreach (var o in this.Outlets)
		//	{
		//		if (o.Outlet == outlet)
		//		{
		//			agendaOutlet = o;
		//			break;
		//		}
		//	}
		//	// We don't have it
		//	if (agendaOutlet == null)
		//	{
		//		// Create it
		//		agendaOutlet = new AgendaOutlet(outlet, new List<Activity>());

		//		// Add to the collection
		//		this.Outlets.Add(agendaOutlet);
		//	}

		//	// Insert into the collection
		//	agendaOutlet.Activities.Add(newActivity);

		//	this.ActivityAdded?.Invoke(this, new ActivityEventArgs(newActivity));

		//	return newActivity;
		//}

		public PermissionResult CanCancel(Activity activity, ActivityCancelReason cancelReason)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			if (activity.Status == null)
			{
				return PermissionResult.Deny(@"CannotCancelInactiveActivity");
			}
			// Check the day
			// TODO : !!!
			//var visitDay = this.DataProvider.GetVisitDay(DateTime.Today);

			return PermissionResult.Allow;
		}

		//public PermissionResult CanCancel(DateTime date)
		//{
		//	var visitDay = this.DataProvider.GetVisitDay(DateTime.Today);

		//	return PermissionResult.Allow;
		//}

		//public PermissionResult CanClose(Activity activity)
		//{
		//	if (activity == null) throw new ArgumentNullException(nameof(activity));

		//	// Check the day
		//	// Check the status
		//	// Check cancel reason
		//	// TODO : !!!

		//	return PermissionResult.Allow;
		//}

		public void Cancel(Activity activity, ActivityCancelReason cancelReason)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : !!!

			this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		}

		//public void CancelDay(ActivityCancelReason cancelReason)
		//{
		//	// TODO : Update activity in the database

		//	var canCancel = false;

		//	// TODO : !!!
		//	//var cancelOperation = new CalendarCancelOperation(new CalendarDataProvider(this.Context.DbContextCreator));
		//	//cancelOperation.CancelActivities(new[] { activityViewModel.Model }, cancelReason, a =>
		//	//{
		//	//	var aid = a.Id;

		//	//	var activities = this.Activities;
		//	//	for (var i = 0; i < activities.Count; i++)
		//	//	{
		//	//		var activity = activities[i];
		//	//		if (activity.Model.Id == aid)
		//	//		{
		//	//			activities[i] = new ActivityViewModel(this, a);
		//	//			break;
		//	//		}
		//	//	}
		//	//});

		//	//this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		//}

		//public void Close(Activity activity, ActivityCloseReason closeReason)
		//{
		//	if (activity == null) throw new ArgumentNullException(nameof(activity));

		//	// TODO : Update activity in the database

		//	// TODO : !!!
		//	//var cancelOperation = new CalendarCancelOperation(new CalendarDataProvider(this.Context.DbContextCreator));
		//	//cancelOperation.CancelActivities(new[] { activityViewModel.Model }, cancelReason, a =>
		//	//{
		//	//	var aid = a.Id;

		//	//	var activities = this.Activities;
		//	//	for (var i = 0; i < activities.Count; i++)
		//	//	{
		//	//		var activity = activities[i];
		//	//		if (activity.Model.Id == aid)
		//	//		{
		//	//			activities[i] = new ActivityViewModel(this, a);
		//	//			break;
		//	//		}
		//	//	}
		//	//});

		//	this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		//}

		//public void ChangeStartTimeAsync(Activity activity, DateTime dateTime)
		//{
		//	if (activity == null) throw new ArgumentNullException(nameof(activity));

		//	activity.FromDate = activity.FromDate.Date.Add(dateTime.TimeOfDay);

		//	this.DataProvider.Update(activity);

		//	this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		//}

		private void OnActivityInserted(ActivityEventArgs e)
		{
			this.ActivityInserted?.Invoke(this, e);
		}

		private void OnActivityUpdated(ActivityEventArgs e)
		{
			this.ActivityUpdated?.Invoke(this, e);
		}

		private void OnActivityDeleted(ActivityEventArgs e)
		{
			this.ActivityDeleted?.Invoke(this, e);
		}
	}
}