using System;

namespace Cchbc.Features
{
	public sealed class FeatureData
	{
		public string Context { get; }
		public string Name { get; }
		public TimeSpan TimeSpent { get; }
		public FeatureStepData[] Steps { get; }

		public FeatureData(string context, string name, TimeSpan timeSpent, FeatureStepData[] steps)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (steps == null) throw new ArgumentNullException(nameof(steps));

			this.Context = context;
			this.Name = name;
			this.TimeSpent = timeSpent;
			this.Steps = steps;
		}
	}
}