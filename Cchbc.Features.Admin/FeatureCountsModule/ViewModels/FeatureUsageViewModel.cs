using System;
using Cchbc.Features.Admin.FeatureCountsModule.Objects;

namespace Cchbc.Features.Admin.FeatureCountsModule.ViewModels
{
    public sealed class FeatureUsageViewModel
    {
        public FeatureCount FeatureCount { get; }

        public string Name { get; }
        public string Context { get; }
        public string Count { get; }

        public FeatureUsageViewModel(FeatureCount featureCount)
        {
            if (featureCount == null) throw new ArgumentNullException(nameof(featureCount));

            this.FeatureCount = featureCount;
	        this.Name = featureCount.Name;
	        this.Context = featureCount.Context;
	        this.Count = featureCount.Value.ToString();
        }
    }
}