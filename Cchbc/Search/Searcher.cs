using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public sealed class Searcher<T> where T : ViewObject
	{
		private Func<T, string, bool> TextMatch { get; } = (item, search) => true;
		public SearchOption<T>[] Options { get; } = new SearchOption<T>[0];
		public SearchOption<T> CurrentOption { get; set; }
		public string TextSearch { get; set; } = string.Empty;

		public Searcher(SearchOption<T>[] options)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));

			this.Options = options;
		}

		public Searcher(Func<T, string, bool> textMatch)
		{
			if (textMatch == null) throw new ArgumentNullException(nameof(textMatch));

			this.TextMatch = textMatch;
		}

		public Searcher(SearchOption<T>[] options, Func<T, string, bool> textMatch)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (textMatch == null) throw new ArgumentNullException(nameof(textMatch));

			this.Options = options;
			this.TextMatch = textMatch;
		}

		public IEnumerable<T> Search(ICollection<T> viewItems, string textSearch, SearchOption<T> option)
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

			IEnumerable<T> currentViewItems;

			if (textSearch == string.Empty)
			{
				this.SetupCounts(viewItems);
				currentViewItems = viewItems;
			}
			else
			{
				var filteredByTextViewItems = new List<T>();

				foreach (var item in viewItems)
				{
					if (this.TextMatch(item, textSearch))
					{
						filteredByTextViewItems.Add(item);
					}
				}

				this.SetupCounts(filteredByTextViewItems);
				currentViewItems = filteredByTextViewItems;
			}

			if (option == null)
			{
				foreach (var item in currentViewItems)
				{
					yield return item;
				}
				yield break;
			}
			foreach (var item in currentViewItems)
			{
				if (option.IsMatch(item))
				{
					yield return item;
				}
			}
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