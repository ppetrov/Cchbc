using System;

namespace Cchbc.Features
{
    public sealed class FeatureStep : IDisposable
    {
        public Feature Feature { get; }
        public string Name { get; }
        public int Level { get; }
        public bool IsParent { get; }
        public TimeSpan TimeSpent { get; internal set; }
        public string Details { get; internal set; }

        public FeatureStep(Feature feature, string name, int level, bool isParent, TimeSpan timeSpent)
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (name == null) throw new ArgumentNullException(nameof(name));

            this.Feature = feature;
            this.Name = name;
            this.Level = level;
            this.IsParent = isParent;
            this.TimeSpent = timeSpent;            
        }

        public void Dispose()
        {
            this.Feature.EndStep(this);
        }
    }
}