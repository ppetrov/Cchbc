using System;
using System.Collections.Generic;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc
{
	public class Module<T> where T : IReadOnlyObject
	{
		private T[] ViewItems { get; set; }

		public Sorter<T> Sorter { get; }
		public Searcher<T> Searcher { get; }
		public FilterOption<T>[] FilterOptions { get; }

		public Module(Sorter<T> sorter, Searcher<T> searcher, FilterOption<T>[] filterOptions = null)
		{
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Sorter = sorter;
			this.Searcher = searcher;
			this.FilterOptions = filterOptions;
		}

		public void LoadData(T[] viewItems)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));

			this.ViewItems = viewItems;
			this.Sorter.Sort(this.ViewItems, this.Sorter.CurrentOption);
		}

		public IEnumerable<T> Sort(ICollection<T> currentViewItems, SortOption<T> sortOption)
		{
			if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

			var flag = sortOption.Ascending ?? true;

			// Sort view items
			this.Sorter.Sort(this.ViewItems, sortOption);

			// Sort current view items
			var copy = new T[currentViewItems.Count];
			currentViewItems.CopyTo(copy, 0);
			this.Sorter.Sort(copy, sortOption);

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
			foreach (var option in this.Sorter.Options)
			{
				if (option != sortOption)
				{
					option.Ascending = null;
				}
			}

			// Return current view items sorted
			foreach (var viewItem in copy)
			{
				yield return viewItem;
			}
		}

		public IEnumerable<T> Search(string textSearch, SearchOption<T> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			return this.Searcher.Search(GetInputViewItems(this.ViewItems), textSearch, searchOption);
		}

		private ICollection<T> GetInputViewItems(ICollection<T> viewItems)
		{
			if (this.FilterOptions != null && this.FilterOptions.Length > 0)
			{
				viewItems = new List<T>();

				foreach (var item in this.ViewItems)
				{
					var include = true;

					foreach (var filter in this.FilterOptions)
					{
						if (filter.IsSelected)
						{
							include &= filter.IsMatch(item);
							if (!include)
							{
								break;
							}
						}
					}

					if (include)
					{
						viewItems.Add(item);
					}
				}
			}

			return viewItems;
		}
	}
}