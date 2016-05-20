using System;

namespace Cchbc.Features.Objects
{
	public sealed class RelativeTimePeriod
	{
		public TimeSpan TimeOffset { get; }
		public RelativeTimeType Type { get; }

		public RelativeTimePeriod(TimeSpan timeOffset, RelativeTimeType type)
		{
			TimeOffset = timeOffset;
			Type = type;
		}

		public RangeTimePeriod ToRange(DateTime origin)
		{
			var fromDate = origin;
			var toDate = origin;

			switch (this.Type)
			{
				case RelativeTimeType.Past:
					fromDate = origin.Add(-this.TimeOffset);
					break;
				case RelativeTimeType.Future:
					toDate = origin.Add(this.TimeOffset);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return new RangeTimePeriod(fromDate, toDate);
		}
	}
}