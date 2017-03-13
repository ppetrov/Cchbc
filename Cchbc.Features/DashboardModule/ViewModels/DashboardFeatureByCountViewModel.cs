using System;

namespace Cchbc.Features.DashboardModule.ViewModels
{
	public sealed class DashboardFeatureByCountViewModel : ViewModel
	{
		public string Name { get; }
		public int Count { get; }

		public DashboardFeatureByCountViewModel(DashboardFeatureByCount feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Name = feature.Feature.Name + @"(" + feature.Context.Name + @")";
			this.Count = feature.Count;
		}
	}
}