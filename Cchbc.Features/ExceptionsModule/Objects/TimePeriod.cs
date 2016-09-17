using System;
using Cchbc.Features.ExceptionsModule.Rows;

namespace Cchbc.Features.ExceptionsModule.Objects
{
	public sealed class TimePeriod
	{
		public TimePeriodRow Row { get; }
		public string Name { get; }

		public TimePeriod(TimePeriodRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}