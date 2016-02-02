using System;

namespace Cchbc.Features
{
	public sealed class FeatureStep
	{
		public string Name { get; }
		public string Details { get; set; }
		public TimeSpan TimeSpent { get; set; }

		public FeatureStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.Details = string.Empty;
			this.TimeSpent = TimeSpan.Zero;
		}
	}
}