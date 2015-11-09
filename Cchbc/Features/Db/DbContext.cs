using System;
using Cchbc.Objects;

namespace Cchbc.Features.Db
{
	public sealed class DbContext : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

		public DbContext(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}