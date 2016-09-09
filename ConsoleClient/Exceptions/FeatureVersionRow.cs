using System;

namespace ConsoleClient.Exceptions
{
	public sealed class FeatureVersionRow
	{
		public long Id { get; }
		public string Name { get; }

		public FeatureVersionRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}