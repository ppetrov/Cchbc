using System;

namespace Cchbc.App.ArticlesModule.Objects
{
	public sealed class Brand
	{
		public static readonly Brand Empty = new Brand(-1, string.Empty);

		public long Id { get; set; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}