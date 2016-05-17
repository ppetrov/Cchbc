using System;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class DashboardFeatureByCountViewModel : ViewModel
	{
		public string Name { get; }
		public int Count { get; }

		public DashboardFeatureByCountViewModel(DashboardFeatureByCount feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Name = feature.Name;
			this.Count = feature.Count;
		}
	}
}