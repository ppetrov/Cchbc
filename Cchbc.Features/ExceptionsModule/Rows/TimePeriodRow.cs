using System;

namespace Cchbc.Features.ExceptionsModule.Rows
{
	public sealed class TimePeriodRow
	{
		public TimeSpan? TimeOffset { get; }
		public string Name { get; }

		public TimePeriodRow(string name, TimeSpan? timeOffset)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.TimeOffset = timeOffset;
			this.Name = name;
		}
	}
}