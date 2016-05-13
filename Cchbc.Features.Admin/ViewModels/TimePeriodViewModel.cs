using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.ViewModels
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
    }
}