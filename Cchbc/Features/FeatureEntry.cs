using System;
using System.Linq;

namespace Cchbc.Features
{
	public sealed class FeatureEntry
	{
		public string Context { get; }
		public string Name { get; }
		public string Details { get; }
		public TimeSpan TimeSpent { get; }
		public FeatureEntryStep[] Steps { get; }

		public FeatureEntry(string context, string name, string details, TimeSpan timeSpent)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Context = context;
			this.Name = name;
			this.Details = details;
			this.TimeSpent = timeSpent;
			this.Steps = Enumerable.Empty<FeatureEntryStep>().ToArray();
		}

		public FeatureEntry(string context, string name, string details, TimeSpan timeSpent, FeatureEntryStep[] entrySteps)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (details == null) throw new ArgumentNullException(nameof(details));
			if (entrySteps == null) throw new ArgumentNullException(nameof(entrySteps));

			this.Context = context;
			this.Name = name;
			this.Details = details;
			this.TimeSpent = timeSpent;
			this.Steps = entrySteps;
		}
	}
}