using System;

namespace Cchbc.Features
{
	public sealed class FeatureStep : IDisposable
	{
		public Feature Feature { get; }
		public string Name { get; }
		public int Level { get; }
		public TimeSpan TimeSpent { get; private set; }

		public FeatureStep(Feature feature, string name, int level, TimeSpan timeSpent)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Feature = feature;
			this.Name = name;
			this.Level = level;
			this.TimeSpent = timeSpent;
		}

		public void Dispose()
		{
			this.TimeSpent = this.Feature.Elapsed - this.TimeSpent;
			this.Feature.EndStep(this);
		}
	}
}