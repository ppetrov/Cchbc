using System;

namespace Cchbc
{
	public sealed class FeatureStep
	{
		public string Name { get; }
		public TimeSpan TimeSpent { get; set; }

		public FeatureStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}
}