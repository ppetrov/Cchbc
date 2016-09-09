using System;

namespace ConsoleClient.Exceptions
{
	public sealed class FeatureUser
	{
		public FeatureUserRow Row { get; }
		public string Name { get; }

		public FeatureUser(FeatureUserRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}