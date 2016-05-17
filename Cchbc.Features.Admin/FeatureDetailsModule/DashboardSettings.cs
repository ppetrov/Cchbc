using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class DashboardSettings
	{
		public int MaxUsers { get; } = 5;
		public int MaxMostUsedFeatures { get; set; } = 10;
		public int VersionsChartSamples { get; } = 10;
		public int ExceptionsChartSamples { get; } = 30;
		public RelativeTimePeriod ExceptionsRelativeTimePeriod { get; } = new RelativeTimePeriod(TimeSpan.FromDays(2), RelativeTimeType.Past);

	}
}