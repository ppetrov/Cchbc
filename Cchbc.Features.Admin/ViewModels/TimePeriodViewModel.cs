using System;
using Cchbc.Features.Objects;

namespace Cchbc.Features.ViewModels
{
    public sealed class TimePeriodViewModel
    {
        public string Description { get; }
        public RangeTimePeriod RangeTimePeriod { get; }

        public TimePeriodViewModel(string description, RangeTimePeriod rangeTimePeriod)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (rangeTimePeriod == null) throw new ArgumentNullException(nameof(rangeTimePeriod));

            this.Description = description;
            this.RangeTimePeriod = rangeTimePeriod;
        }

		public static TimePeriodViewModel[] GetStandardPeriods()
		{
			var today = DateTime.Today;
			var toDate = today.AddDays(1);
			return new[]
			{
				new TimePeriodViewModel(@"Today", new RangeTimePeriod(today, toDate)),
				new TimePeriodViewModel(@"Last 7 days", new RangeTimePeriod(toDate.AddDays(-7), toDate)),
				new TimePeriodViewModel(@"Last 30 days", new RangeTimePeriod(toDate.AddDays(-30), toDate)),
			};
		}
	}
}