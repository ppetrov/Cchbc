using System;
using Cchbc.Objects;

namespace Cchbc.Sort
{
	public sealed class SortOption<T> : ViewObject
	{
		public string Name { get; }
		public Func<T, T, int> Comparison { get; }
		public bool IsDefault { get; }

		private bool? _ascending;
		public bool? Ascending
		{
			get { return _ascending; }
			set { this.SetField(ref _ascending, value); }
		}

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