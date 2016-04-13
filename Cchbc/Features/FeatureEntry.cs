using System;
using System.Collections.Generic;

namespace Cchbc.Features
{
	public sealed class FeatureEntry : DbEntry
	{
		public string Details { get; }
		public TimeSpan TimeSpent { get; }
		public ICollection<FeatureEntryStep> Steps { get; }

		public FeatureEntry(string context, string name, string details, TimeSpan timeSpent, ICollection<FeatureEntryStep> steps)
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