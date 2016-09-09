using System;

namespace ConsoleClient.Exceptions
{
	public sealed class FeatureVersion
	{
		public FeatureVersionRow Row { get; }
		public string Name { get; }

		public FeatureVersion(FeatureVersionRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}