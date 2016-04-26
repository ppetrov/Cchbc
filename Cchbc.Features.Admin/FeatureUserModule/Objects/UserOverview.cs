using System;
using System.Collections.Generic;

namespace Cchbc.Features.Admin.FeatureUserModule.Objects
{
	public sealed class UserOverview
	{
		public string Name { get; }
		public List<FeatureCountByPeriod> Features { get; }
		public List<FeatureCountByPeriod> Exceptions { get; }

		public UserOverview(string name, IndexedTimePeriod[] timePeriods)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (timePeriods == null) throw new ArgumentNullException(nameof(timePeriods));

			this.Name = name;
			this.Features = new List<FeatureCountByPeriod>(timePeriods.Length);
			this.Exceptions = new List<FeatureCountByPeriod>(timePeriods.Length);

			foreach (var period in timePeriods)
			{
				this.Features.Add(new FeatureCountByPeriod(period));
				this.Exceptions.Add(new FeatureCountByPeriod(period));
			}
		}
	}
}