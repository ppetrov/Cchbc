using System;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboardFeatureByTime
	{
		public long Id { get; }
		public string Name { get; }
		public TimeSpan TimeSpent { get; }

		public DashboardFeatureByTime(long id, string name, TimeSpan timeSpent)
		{
			this.Id = id;
			this.Name = name;
			this.TimeSpent = timeSpent;
		}
	}
}