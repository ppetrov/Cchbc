using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc
{
	public class ReadOnlyModule<T> where T : ViewObject
	{
		private T[] ViewItems { get; set; }

		public ILogger Logger { get; }
		public Func<ILogger, Task<T[]>> DataLoader { get; }
		public Sorter<T> Sorter { get; }
		public Searcher<T> Searcher { get; }
		public FilterOption<T>[] FilterOptions { get; set; }

		public ReadOnlyModule(ILogger logger, Func<ILogger, Task<T[]>> dataLoader, Sorter<T> sorter, Searcher<T> searcher)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (dataLoader == null) throw new ArgumentNullException(nameof(dataLoader));
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Logger = logger;
			this.DataLoader = dataLoader;
			this.Sorter = sorter;
			this.Searcher = searcher;
		}

		public async Task LoadDataAsync()
		{
			this.ViewItems = await this.DataLoader(this.Logger);
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
			if (this.FilterOptions != null)
			{
				var selectedFilterOptions = this.FilterOptions.Where(f => f.IsSelected).ToList();
				if (selectedFilterOptions.Count > 0)
				{
					viewItems = new List<T>();
					foreach (var item in this.ViewItems)
					{
						var include = true;

						foreach (var filter in selectedFilterOptions)
						{
							include &= filter.IsMatch(item);
							if (!include)
							{
								break;
							}
						}

						if (include)
						{
							viewItems.Add(item);
						}
					}
				}
			}

			return viewItems;
		}
	}
}