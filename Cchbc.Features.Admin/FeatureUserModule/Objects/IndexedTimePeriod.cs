using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.FeatureUserModule.Objects
{
	public sealed class IndexedTimePeriod
	{
		public int Index { get; }
		public TimePeriod TimePeriod { get; }

		public IndexedTimePeriod(int index, TimePeriod timePeriod)
		{
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			this.Index = index;
			this.TimePeriod = timePeriod;
		}
	}
}