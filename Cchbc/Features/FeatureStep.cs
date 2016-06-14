using System;

namespace Cchbc.Features
{
    public sealed class FeatureStep : IDisposable
    {
        public Feature Feature { get; }
        public string Name { get; }
        public TimeSpan TimeSpent { get; internal set; }
        public string Details { get; internal set; }

        public FeatureStep(Feature feature, string name, TimeSpan timeSpent)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (name == null) throw new ArgumentNullException(nameof(name));

            this.Feature = feature;
            this.Name = name;
            this.TimeSpent = timeSpent;
        }

        public void Dispose()
        {
            this.Feature.EndStep(this);
        }
    }
}