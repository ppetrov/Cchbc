using System;

namespace Cchbc.App.ArticlesModule.Objects
{
	public sealed class Flavor
	{
		public static readonly Flavor Empty = new Flavor(-1, string.Empty);

		public long Id { get; set; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}