using System;

namespace Cchbc.Features.ExceptionsModule.Rows
{
	public sealed class TimePeriodRow
	{
		public string Name { get; }
		public TimeSpan TimeOffset { get; }
		public int ChartSamples { get; }

		public TimePeriodRow(string name, TimeSpan timeOffset, int chartSamples)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.TimeOffset = timeOffset;
			this.Name = name;
			this.ChartSamples = chartSamples;
		}
	}
}