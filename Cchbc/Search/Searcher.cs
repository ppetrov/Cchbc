using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public sealed class Searcher<T> where T : ViewObject
	{
		private Func<T, string, bool> IsMatch { get; } = (item, search) => true;
		public SearchOption<T>[] Options { get; } = new SearchOption<T>[0];
		public SearchOption<T> CurrentOption { get; set; }
		public string TextSearch { get; set; } = string.Empty;

		public Searcher(SearchOption<T>[] options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			this.Options = options;
		}

		public Searcher(Func<T, string, bool> isMatch)
		{
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.IsMatch = isMatch;
		}

		public Searcher(SearchOption<T>[] options, Func<T, string, bool> isMatch)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.Options = options;
			this.IsMatch = isMatch;
		}

		public List<T> FindAll(T[] viewItems, string textSearch, SearchOption<T> option)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			this.CurrentOption = option;
			if (option != null)
			{
				option.IsSelected = true;
			}
			foreach (var o in this.Options)
			{
				if (o != option)
				{
					o.IsSelected = false;
				}
			}

			this.TextSearch = textSearch;

			var items = new List<T>();

			// Filter by text
			if (textSearch != string.Empty)
			{
				foreach (var item in viewItems)
				{
					if (this.IsMatch(item, textSearch))
					{
						items.Add(item);
					}
				}
			}
			else
			{
				items.AddRange(viewItems);
			}

			this.SetupCounts(items);

			if (option != null)
			{
				for (var i = 0; i < items.Count; i++)
				{
					var item = items[i];
					if (!option.IsMatch(item))
					{
						// Mark the item for removal
						items[i] = null;
					}
				}

				// Remove all marked items
				items.RemoveAll(v => v == null);
			}

			return items;
		}

		public void SetupCounts(IEnumerable<T> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			foreach (var option in this.Options)
			{
				var count = 0;
				foreach (var item in items)
				{
					if (option.IsMatch(item))
					{
						count++;
					}
				}
				option.Count = count;
			}
		}
	}
}