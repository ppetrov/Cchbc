using System;
using Cchbc.Features.Objects;

namespace Cchbc.Features.DashboardModule
{
	public sealed class DashboardSettings
	{
		public static readonly DashboardSettings Default = new DashboardSettings(10, 5, 10, 10, 24, new RelativeTimePeriod(TimeSpan.FromHours(1), RelativeTimeType.Past));

		public int MaxUsers { get; }
		public int MaxVersions { get; }
		public int MaxMostUsedFeatures { get; }
		public int MaxLeastUsedFeatures { get; }

		public int ExceptionsChartEntries { get; }
		public RelativeTimePeriod ExceptionsRelativeTimePeriod { get; }

		public DashboardSettings(int maxUsers, int maxVersions, int maxMostUsedFeatures, int maxLeastUsedFeatures, int exceptionsChartEntries, RelativeTimePeriod exceptionsRelativeTimePeriod)
		{
			this.MaxUsers = maxUsers;
			this.MaxVersions = maxVersions;
			this.MaxMostUsedFeatures = maxMostUsedFeatures;
			this.MaxLeastUsedFeatures = maxLeastUsedFeatures;
			this.ExceptionsChartEntries = exceptionsChartEntries;
			this.ExceptionsRelativeTimePeriod = exceptionsRelativeTimePeriod;
		}
	}
}