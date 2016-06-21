using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureStepRow
	{
		public readonly int Id;
		public readonly string Name;

		public DbFeatureStepRow(int id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}