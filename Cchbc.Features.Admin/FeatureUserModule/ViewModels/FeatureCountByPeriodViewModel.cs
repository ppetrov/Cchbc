using System;
using Cchbc.Features.Admin.ViewModels;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureUserModule.ViewModels
{
	public sealed class FeatureCountByPeriodViewModel : ViewModel
	{
		public TimePeriodViewModel TimePeriod { get; }
		public int Count { get; }

		public FeatureCountByPeriodViewModel(TimePeriodViewModel timePeriod, int count)
		{
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			this.TimePeriod = timePeriod;
			this.Count = count;
		}
	}
}