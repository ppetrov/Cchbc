using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.FeatureUserModule.Objects
{
	public sealed class IndexedTimePeriod
	{
		public int Index { get; }
		public RangeTimePeriod RangeTimePeriod { get; }

		public IndexedTimePeriod(int index, RangeTimePeriod rangeTimePeriod)
		{
			if (rangeTimePeriod == null) throw new ArgumentNullException(nameof(rangeTimePeriod));

			this.Index = index;
			this.RangeTimePeriod = rangeTimePeriod;
		}
	}
}