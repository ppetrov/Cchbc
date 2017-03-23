using System;
using Cchbc;

namespace iFSA.ArchitectureModule
{
	public sealed class SortOption<T> : ViewModel
	{
		public string Name { get; }
		public Comparison<T> Comparer { get; }
		private bool? _ascending;
		public bool? Ascending
		{
			get { return _ascending; }
			set { this.SetProperty(ref _ascending, value); }
		}

		public SortOption(string name, Comparison<T> comparer)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (comparer == null) throw new ArgumentNullException(nameof(comparer));

			this.Name = name;
			this.Comparer = comparer;
		}
	}
}