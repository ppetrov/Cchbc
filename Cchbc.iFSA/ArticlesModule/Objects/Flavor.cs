using System;

namespace iFSA.ArticlesModule.Objects
{
	public sealed class Flavor
	{
		public static readonly Flavor Empty = new Flavor(0, string.Empty);

		public long Id { get; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}