using System;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureStepRow
	{
		public readonly long Id;
		public readonly string Name;

		public DbFeatureStepRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}