using System;

namespace Cchbc.Features
{
	public sealed class FeatureEntry : DbEntry
	{
		public string Details { get; }
		public TimeSpan TimeSpent { get; }
		public FeatureEntryStep[] Steps { get; }

		public FeatureEntry(string context, string name, string details, TimeSpan timeSpent, FeatureEntryStep[] steps)
			: base(context, name)
		{
			if (details == null) throw new ArgumentNullException(nameof(details));
			if (steps == null) throw new ArgumentNullException(nameof(steps));

			this.Details = details;
			this.TimeSpent = timeSpent;
			this.Steps = steps;
		}
	}
}