using System;

namespace Atos.iFSA.LoginModule2
{
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
}