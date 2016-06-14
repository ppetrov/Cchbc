using System;

namespace Cchbc.Features
{
	public sealed class FeatureStep
	{
		public string Name { get; }
		public TimeSpan TimeSpent { get; internal set; }
		public string Details { get; internal set; }

		public FeatureStep(string name, TimeSpan timeSpent)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.TimeSpent = timeSpent;
		}
	}
}