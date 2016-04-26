using System;

namespace Cchbc.Features.Admin.FeatureUserModule.Objects
{
	public sealed class FeatureCountByPeriod
	{
		public IndexedTimePeriod IndexedTimePeriod { get; }
		public int Count { get; set; }

		public FeatureCountByPeriod(IndexedTimePeriod indexedTimePeriod)
		{
			if (indexedTimePeriod == null) throw new ArgumentNullException(nameof(indexedTimePeriod));

			this.IndexedTimePeriod = indexedTimePeriod;
		}
	}
}