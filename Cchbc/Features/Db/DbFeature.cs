using System;
using Cchbc.Objects;

namespace Cchbc.Features.Db
{
	public sealed class DbFeature : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }
		public DbContext Context { get; }

		public DbFeature(long id, string name, DbContext context)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Id = id;
			this.Name = name;
			this.Context = context;
		}
	}
}