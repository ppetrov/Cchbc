using System;
using Cchbc.Features.Admin.FeatureUsageModule.Objects;

namespace Cchbc.Features.Admin.FeatureUsageModule.ViewModels
{
    public sealed class TimePeriodViewModel
    {
        public string Name { get; }
        public TimePeriod TimePeriod { get; }

        public TimePeriodViewModel(string name, TimePeriod timePeriod)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            this.Name = name;
            this.TimePeriod = timePeriod;
        }
    }
}