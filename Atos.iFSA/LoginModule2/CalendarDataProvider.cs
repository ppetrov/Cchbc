using System;
using System.Collections.Generic;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.LoginModule2
{
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