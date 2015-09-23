using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public sealed class Searcher<T> : ViewObject where T : ViewObject
	{
		private Func<T, string, bool> IsMatch { get; } = (item, search) => true;
		public SearcherOption<T>[] Options { get; } = new SearcherOption<T>[0];

		private SearcherOption<T> _currentOption;
		public SearcherOption<T> CurrentOption
		{
			get { return _currentOption; }
			set { this.SetField(ref _currentOption, value); }
		}

		private string _search = string.Empty;
		public string Search
		{
			get { return _search; }
			set { this.SetField(ref _search, value); }
		}

		public Searcher(SearcherOption<T>[] options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			this.Options = options;
		}

		public Searcher(Func<T, string, bool> isMatch)
		{
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.IsMatch = isMatch;
		}

		public Searcher(SearcherOption<T>[] options, Func<T, string, bool> isMatch)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.Options = options;
			this.IsMatch = isMatch;
		}

		public List<T> FindAll(T[] viewItems, string search, SearcherOption<T> option)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (search == null) throw new ArgumentNullException(nameof(search));

			this.CurrentOption = option;
			this.Search = search;

			var items = new List<T>();

			// Filter by text
			if (search != string.Empty)
			{
				foreach (var item in viewItems)
				{
					if (this.IsMatch(item, search))
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

		private void SetupCounts(IEnumerable<T> items)
		{
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