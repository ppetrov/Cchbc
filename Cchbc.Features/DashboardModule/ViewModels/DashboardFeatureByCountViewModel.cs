using System;
using Cchbc.Objects;

namespace Cchbc.Features.DashboardModule.ViewModels
{
	public sealed class DashboardFeatureByCountViewModel : ViewModel
	{
		public string Context { get; }
		public string Name { get; }
		public int Count { get; }

		public DashboardFeatureByCountViewModel(DashboardFeatureByCount feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Context = feature.Context.Name;
			this.Name = feature.Feature.Name;
			this.Count = feature.Count;
		}
	}
}