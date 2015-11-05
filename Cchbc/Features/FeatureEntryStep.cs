using System;

namespace Cchbc.Features
{
	public sealed class FeatureEntryStep
	{
		public string Name { get; }
		public TimeSpan TimeSpent { get; }
		public string Details { get; }

		public FeatureEntryStep(string name, TimeSpan timeSpent)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.TimeSpent = timeSpent;
			this.Details = string.Empty;
		}

		public FeatureEntryStep(string name, TimeSpan timeSpent, string details)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Name = name;
			this.Details = details;
			this.TimeSpent = timeSpent;
		}
	}
}