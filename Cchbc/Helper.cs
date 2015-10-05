using System;
using System.Collections.Generic;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc
{
	public class Helper<T, TViewItem> where T : IDbObject where TViewItem : ViewItem<T>
	{
		private TViewItem[] ViewItems { get; set; }

		public Sorter<TViewItem> Sorter { get; }
		public Searcher<TViewItem> Searcher { get; }
		public FilterOption<TViewItem>[] FilterOptions { get; }

		public Helper(Sorter<TViewItem> sorter, Searcher<TViewItem> searcher, FilterOption<TViewItem>[] filterOptions = null)
		{
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Sorter = sorter;
			this.Searcher = searcher;
			this.FilterOptions = filterOptions;
		}

		public void LoadData(TViewItem[] viewItems)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));

			this.ViewItems = viewItems;
			this.Sorter.Sort(this.ViewItems, this.Sorter.CurrentOption);
		}

		public IEnumerable<TViewItem> Sort(ICollection<TViewItem> currentViewItems, SortOption<TViewItem> sortOption)
		{
			if (currentViewItems == null) throw new ArgumentNullException(nameof(currentViewItems));
			if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

			var flag = sortOption.Ascending ?? true;

			// Sort view items
			this.Sorter.Sort(this.ViewItems, sortOption);

			// Sort current view items
			var copy = new TViewItem[currentViewItems.Count];
			currentViewItems.CopyTo(copy, 0);
			this.Sorter.Sort(copy, sortOption);
			this.Sorter.SetupFlag(sortOption, flag);

			// Return current view items sorted
			foreach (var viewItem in copy)
			{
				yield return viewItem;
			}
		}

		public IEnumerable<TViewItem> Search(string textSearch, SearchOption<TViewItem> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			return this.Searcher.Search(GetFilteredViewItems(this.ViewItems), textSearch, searchOption);
		}

		private ICollection<TViewItem> GetFilteredViewItems(ICollection<TViewItem> viewItems)
		{
			if (this.FilterOptions != null && this.FilterOptions.Length > 0)
			{
				viewItems = new List<TViewItem>();

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