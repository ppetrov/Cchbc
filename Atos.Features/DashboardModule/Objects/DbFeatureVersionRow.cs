using System;

namespace Atos.Features.DashboardModule.Objects
{
	public sealed class DbFeatureVersionRow
	{
		public readonly int Id;
		public readonly string Name;

		public DbFeatureVersionRow(int id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}