using System;
using System.Collections.Generic;

namespace Atos.iFSA.LoginModule2
{
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
}