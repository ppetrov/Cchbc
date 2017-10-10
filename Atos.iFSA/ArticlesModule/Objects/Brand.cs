using System;

namespace Atos.iFSA.ArticlesModule.Objects
{
	public sealed class Brand
	{
		public static readonly Brand Empty = new Brand(0, string.Empty);

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