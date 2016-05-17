using System;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class DashboardFeatureByTimeViewModel : ViewModel
	{
		public string Name { get; }
		public TimeSpan TimeSpent { get; }

		public DashboardFeatureByTimeViewModel(DashboardFeatureByTime feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Name = feature.Name;
			this.TimeSpent = feature.TimeSpent;
		}
	}
}