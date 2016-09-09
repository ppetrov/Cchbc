using System;

namespace ConsoleClient.Exceptions
{
	public sealed class FeatureExceptionRow
	{
		public long Id { get; }
		public string Name { get; }

		public FeatureExceptionRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}