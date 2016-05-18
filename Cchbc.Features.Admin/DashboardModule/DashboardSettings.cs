using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboardSettings
	{
		public static readonly DashboardSettings Default = new DashboardSettings(10, 10, 10, 10, 24, new RelativeTimePeriod(TimeSpan.FromHours(1), RelativeTimeType.Past));

		public int MaxUsers { get; }
		public int MaxMostUsedFeatures { get; }
		public int MaxLeastUsedFeatures { get; }

		public int VersionsChartEntries { get; }
		public int ExceptionsChartEntries { get; }
		public RelativeTimePeriod ExceptionsRelativeTimePeriod { get; }

		public DashboardSettings(int maxUsers, int maxMostUsedFeatures, int maxLeastUsedFeatures, int versionsChartEntries, int exceptionsChartEntries, RelativeTimePeriod exceptionsRelativeTimePeriod)
		{
			this.MaxUsers = maxUsers;
			this.MaxMostUsedFeatures = maxMostUsedFeatures;
			this.MaxLeastUsedFeatures = maxLeastUsedFeatures;
			this.VersionsChartEntries = versionsChartEntries;
			this.ExceptionsChartEntries = exceptionsChartEntries;
			this.ExceptionsRelativeTimePeriod = exceptionsRelativeTimePeriod;
		}
	}
}