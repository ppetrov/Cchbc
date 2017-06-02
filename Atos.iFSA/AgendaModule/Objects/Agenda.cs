using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Atos;
using Atos.Logs;
using Atos.Validation;
using iFSA.AgendaModule.Data;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Objects
{
	public sealed class ActivityEventArgs : EventArgs
	{
		public Activity Activity { get; }

		public ActivityEventArgs(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));
			this.Activity = activity;
		}
	}

	public sealed class OutletImageEventArgs : EventArgs
	{
		public OutletImage OutletImage { get; }

		public OutletImageEventArgs(OutletImage outletImage)
		{
			if (outletImage == null) throw new ArgumentNullException(nameof(outletImage));

			this.OutletImage = outletImage;
		}
	}

	public sealed class Agenda
	{
		private AgendaDataProvider DataProvider { get; }
		private CancellationTokenSource _cts = new CancellationTokenSource();

		public List<AgendaOutlet> Outlets { get; } = new List<AgendaOutlet>();

		public event EventHandler<OutletImageEventArgs> OutletImageDownloaded;
		public event EventHandler<ActivityEventArgs> ActivityAdded;
		public event EventHandler<ActivityEventArgs> ActivityDeleted;
		public event EventHandler<ActivityEventArgs> ActivityUpdated;

		public Agenda(AgendaDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void LoadDay(MainContext mainContext, User user, DateTime dateTime)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.Outlets.Clear();
			this.Outlets.AddRange(this.DataProvider.GetAgendaOutlets(mainContext, user, dateTime));

			var outlets = new Outlet[this.Outlets.Count];
			for (var index = 0; index < this.Outlets.Count; index++)
			{
				outlets[index] = this.Outlets[index].Outlet;
			}

			// Cancel any pending Images Load
			this._cts.Cancel();

			// Start new Images Load
			this._cts = new CancellationTokenSource();

			Task.Run(() =>
			{
				try
				{
					var cts = this._cts;
					foreach (var outlet in outlets)
					{
						if (cts.IsCancellationRequested)
						{
							break;
						}
						var outletImage = this.DataProvider.GetDefaultOutletImage(mainContext, outlet);
						if (outletImage != null)
						{
							this.OutletImageDownloaded?.Invoke(this, new OutletImageEventArgs(outletImage));
						}
					}
				}
				catch (Exception ex)
				{
					mainContext.Log(ex.ToString(), LogLevel.Error);
				}
			}, this._cts.Token);
		}

		public PermissionResult CanCreate(Outlet outlet, ActivityType activityType)
		{
			if (outlet == null)
			{
				return PermissionResult.Deny(@"MissingOutlet");
			}
			if (activityType == null)
			{
				return PermissionResult.Deny(@"MissingActivityType");
			}
			if (outlet.Id > 0 && activityType.Id > 0)
			{
				return PermissionResult.Confirm(@"Confirm activity type for this outlet?");
			}
			return PermissionResult.Allow;
		}

		public PermissionResult CanCancel(Activity activity, ActivityCancelReason cancelReason)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			if (activity.Status == null)
			{
				return PermissionResult.Deny(@"CannotCancelInactiveActivity");
			}
			// Check the day
			// TODO : !!!
			var visitDay = this.DataProvider.GetVisitDay(DateTime.Today);

			return PermissionResult.Allow;
		}

		public PermissionResult CanCancel(DateTime date)
		{
			var visitDay = this.DataProvider.GetVisitDay(DateTime.Today);

			return PermissionResult.Allow;
		}

		public PermissionResult CanClose(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// Check the day
			// Check the status
			// Check cancel reason
			// TODO : !!!

			return PermissionResult.Allow;
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

		public Activity Create(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// Save activity to db
			var newActivity = this.DataProvider.Insert(activity);

			var outlet = activity.Outlet;

			// Find item by outlet
			var agendaOutlet = default(AgendaOutlet);
			foreach (var o in this.Outlets)
			{
				if (o.Outlet == outlet)
				{
					agendaOutlet = o;
					break;
				}
			}
			// We don't have it
			if (agendaOutlet == null)
			{
				// Create it
				agendaOutlet = new AgendaOutlet(outlet, new List<Activity>());

				// Add to the collection
				this.Outlets.Add(agendaOutlet);
			}

			// Insert into the collection
			agendaOutlet.Activities.Add(newActivity);

			this.ActivityAdded?.Invoke(this, new ActivityEventArgs(newActivity));

			return newActivity;
		}

		public void Cancel(Activity activity, ActivityCancelReason cancelReason)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : !!!

			this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		}

		public void CancelDay(ActivityCancelReason cancelReason)
		{
			// TODO : Update activity in the database

			var canCancel = false;

			// TODO : !!!
			//var cancelOperation = new CalendarCancelOperation(new CalendarDataProvider(this.Context.DbContextCreator));
			//cancelOperation.CancelActivities(new[] { activityViewModel.Model }, cancelReason, a =>
			//{
			//	var aid = a.Id;

			//	var activities = this.Activities;
			//	for (var i = 0; i < activities.Count; i++)
			//	{
			//		var activity = activities[i];
			//		if (activity.Model.Id == aid)
			//		{
			//			activities[i] = new ActivityViewModel(this, a);
			//			break;
			//		}
			//	}
			//});

			//this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		}

		public void Close(Activity activity, ActivityCloseReason closeReason)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : Update activity in the database

			// TODO : !!!
			//var cancelOperation = new CalendarCancelOperation(new CalendarDataProvider(this.Context.DbContextCreator));
			//cancelOperation.CancelActivities(new[] { activityViewModel.Model }, cancelReason, a =>
			//{
			//	var aid = a.Id;

			//	var activities = this.Activities;
			//	for (var i = 0; i < activities.Count; i++)
			//	{
			//		var activity = activities[i];
			//		if (activity.Model.Id == aid)
			//		{
			//			activities[i] = new ActivityViewModel(this, a);
			//			break;
			//		}
			//	}
			//});

			this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		}

		public void ChangeStartTime(Activity activity, DateTime dateTime)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			activity.FromDate = activity.FromDate.Date.Add(dateTime.TimeOfDay);

			this.DataProvider.Update(activity);

			this.ActivityUpdated?.Invoke(this, new ActivityEventArgs(activity));
		}
	}
}