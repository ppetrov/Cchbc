using System;
using Cchbc.Features.Admin.FeatureUsageModule.Objects;

namespace Cchbc.Features.Admin.FeatureUsageModule.ViewModels
{
    public sealed class FeatureUsageViewModel
    {
        public FeatureUsage FeatureUsage { get; }

        public string Name { get; }
        public string Context { get; }
        public string Count { get; }

        public FeatureUsageViewModel(FeatureUsage featureUsage)
        {
            if (featureUsage == null) throw new ArgumentNullException(nameof(featureUsage));

            this.FeatureUsage = featureUsage;
	        this.Name = featureUsage.Name;
	        this.Context = featureUsage.Context;
	        this.Count = featureUsage.Count.ToString();
        }
    }
}