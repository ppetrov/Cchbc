using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Sort
{
	public sealed class Sorter<T> where T : ViewObject
	{
		public SortOption<T>[] Options { get; private set; }
		public SortOption<T> CurrentOption { get; private set; }

		public Sorter(SortOption<T>[] options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (options.Length == 0) throw new ArgumentOutOfRangeException(nameof(options));

			this.Options = options;

			foreach (var option in options)
			{
				if (option.IsDefault)
				{
					this.CurrentOption = option;
					break;
				}
			}

			if (this.CurrentOption == null)
			{
				this.CurrentOption = options[0];
			}
		}

		public void Sort(T[] items, SortOption<T> option)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (option == null) throw new ArgumentNullException(nameof(option));

			Array.Sort(items, GetComparison(option));
		}

		public void Sort(List<T> items, SortOption<T> option)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (option == null) throw new ArgumentNullException(nameof(option));

			items.Sort(GetComparison(option));
		}

		public void SetupFlag(SortOption<T> sortOption, bool flag)
		{
			if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

			// Set the new flag
			if (sortOption.Ascending.HasValue)
			{
				sortOption.Ascending = !flag;
			}
			else
			{
				sortOption.Ascending = true;
			}

			// Clear all sort options
			foreach (var option in this.Options)
			{
				if (option != sortOption)
				{
					option.Ascending = null;
				}
			}
		}

		private Comparison<T> GetComparison(SortOption<T> option)
		{
			// Set the current option
			this.CurrentOption = option;

			var cmp = new Comparison<T>(option.Comparison);
			if (!(option.Ascending ?? true))
			{
				// Sort in descending order
				cmp = (x, y) => option.Comparison(y, x);
			}
			return cmp;
		}
	}
}