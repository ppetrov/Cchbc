using System;
using System.Linq;
using Cchbc.Objects;

namespace Cchbc.Sort
{
	public sealed class Sorter<T> where T : ViewObject
	{
		public SortOption<T>[] Options { get; private set; }
		public SortOption<T> CurrentOption { get; private set; }
		public bool? Ascending { get; private set; }

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
				this.CurrentOption = options.First();
			}
		}

		public void Sort(T[] items, SortOption<T> option, bool? descending = null)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (option == null) throw new ArgumentNullException(nameof(option));

			var ascending = (this.CurrentOption != option);
			if (descending.HasValue && descending.Value)
			{
				ascending = false;
			}

			// Set the current option
			this.CurrentOption = option;
			this.Ascending = ascending;

			if (ascending)
			{
				// Sort the copy in ascending order
				Array.Sort(items, new Comparison<T>(option.Comparison));
			}
			else
			{
				if (descending ?? false)
				{
					// Sort the copy in descending order
					var cmp = new Comparison<T>((x, y) => option.Comparison(y, x));
					Array.Sort(items, cmp);
				}
				else
				{
					// The array is already sorted, just reverse it.
					Array.Reverse(items);
				}
			}
		}
	}
}