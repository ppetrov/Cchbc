using System;
using Cchbc.Objects;

namespace Cchbc.Sort
{
	public sealed class SortOption<T> where T : ViewObject
	{
		public string Name { get; private set; }
		public Func<T, T, int> Comparison { get; private set; }
		public bool IsDefault { get; private set; }

		public SortOption(string name, Func<T, T, int> comparison, bool isDefault = false)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (comparison == null) throw new ArgumentNullException(nameof(comparison));

			this.Name = name;
			this.Comparison = comparison;
			this.IsDefault = isDefault;
		}
	}
}