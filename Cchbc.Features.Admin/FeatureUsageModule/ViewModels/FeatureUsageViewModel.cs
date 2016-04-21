using System;
using Cchbc.Features.Admin.FeatureUsageModule.Objects;

namespace Cchbc.Features.Admin.FeatureUsageModule.ViewModels
{
    public sealed class FeatureUsageViewModel
    {
        private FeatureUsage FeatureUsage { get; }

        public string Name => this.FeatureUsage.Name;
        public string Context => this.FeatureUsage.Context;
        public int Count => this.FeatureUsage.Count;

        public FeatureUsageViewModel(FeatureUsage featureUsage)
        {
            if (featureUsage == null) throw new ArgumentNullException(nameof(featureUsage));

            this.FeatureUsage = featureUsage;
        }
    }
}