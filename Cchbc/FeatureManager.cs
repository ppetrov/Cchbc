using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cchbc
{
	public sealed class FeatureManager
	{
		private sealed class FeatureEntry
		{
			public string Context { get; }
			public string Name { get; }
			public TimeSpan TimeSpent { get; }

			public FeatureEntry(string context, string name, TimeSpan timeSpent)
			{
				if (context == null) throw new ArgumentNullException(nameof(context));
				if (name == null) throw new ArgumentNullException(nameof(name));

				this.Context = context;
				this.Name = name;
				this.TimeSpent = timeSpent;
			}
		}

		private List<FeatureEntry> Entries { get; } = new List<FeatureEntry>();

		public void Add(string context, Feature feature)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.StopMeasure();

			// Ignore blank features
			if (feature == Feature.None)
			{
				return;
			}

			this.Entries.Add(new FeatureEntry(context, feature.Name, feature.TimeSpent));

			this.Dump();
		}

		private void Dump()
		{
			foreach (var byContextGroup in this.Entries.GroupBy(v => v.Context))
			{
				foreach (var e in byContextGroup)
				{
					Debug.WriteLine(byContextGroup.Key + "->" + e.Name + ", " + e.TimeSpent.TotalMilliseconds + "ms");
				}
				Debug.WriteLine(string.Empty);
			}
		}
	}
}