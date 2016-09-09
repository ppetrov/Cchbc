using System;

namespace ConsoleClient.Exceptions
{
	public sealed class FeatureException
	{
		public FeatureExceptionRow Row { get; }
		public string Name { get; }

		public FeatureException(FeatureExceptionRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}