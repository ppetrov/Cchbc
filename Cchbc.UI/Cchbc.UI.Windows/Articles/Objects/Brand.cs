using System;
using Cchbc.Objects;

namespace Cchbc.ArticlesModule
{
	public sealed class Brand : IReadOnlyObject
	{
		public static readonly Brand Empty = new Brand(-1, string.Empty);

		public long Id { get; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}