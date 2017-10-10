using System;
using System.Collections.Generic;
using System.Linq;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.LoginModule2
{
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
}