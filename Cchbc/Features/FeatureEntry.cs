using System;
using System.Collections.Generic;

namespace Cchbc.Features
{
	public sealed class FeatureEntry
	{
		public string Context { get; }
		public string Name { get; }
		public string Details { get; }
		public TimeSpan TimeSpent { get; }
		public ICollection<FeatureEntryStep> Steps { get; }

		public FeatureEntry(string context, string name, string details, TimeSpan timeSpent, ICollection<FeatureEntryStep> steps)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (details == null) throw new ArgumentNullException(nameof(details));
			if (steps == null) throw new ArgumentNullException(nameof(steps));

			this.Context = context;
			this.Name = name;
			this.Details = details;
			this.TimeSpent = timeSpent;
			this.Steps = steps;
		}
	}
}