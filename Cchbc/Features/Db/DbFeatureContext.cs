using System;
using Cchbc.Objects;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureContext : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

		public DbFeatureContext(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}