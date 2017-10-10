using System;
using System.Collections.Generic;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.LoginModule2
{
	public interface ICalendarDataProvider
	{
		Func<IDbContext> DbContextCreator { get; }

		ICollection<CalendarDay> GetCalendar(User user, DateTime date);

		List<CalendarDay> GetPreviousDays(CalendarDay day);

		bool HasActivitiesForCancel(CalendarDay day);

		void CancelActivities(IDbContext context, ICollection<Activity> activities, CancelReason cancelReason);

		void Cancel(IDbContext context, CalendarDay day, DayStatus cancelStatus);
	}
}