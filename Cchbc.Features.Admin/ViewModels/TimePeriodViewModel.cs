using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.ViewModels
{
    public sealed class TimePeriodViewModel
    {
        public string Description { get; }
        public TimePeriod TimePeriod { get; }

        public TimePeriodViewModel(string description, TimePeriod timePeriod)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            this.Description = description;
            this.TimePeriod = timePeriod;
        }
    }
}