using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public sealed class Searcher<T> : ViewObject where T : ViewObject
	{
		private ObservableCollection<T> ViewItems { get; }
		private Func<T, string, bool> IsMatch { get; }
		public ObservableCollection<SearcherOption<T>> Options { get; }

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

		public Searcher(ObservableCollection<T> viewItems, ObservableCollection<SearcherOption<T>> options)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (options == null) throw new ArgumentNullException(nameof(options));

			this.ViewItems = viewItems;
			this.Options = options;
		}

		public Searcher(ObservableCollection<T> viewItems, Func<T, string, bool> isMatch)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.ViewItems = viewItems;
			this.IsMatch = isMatch;
		}

		public Searcher(ObservableCollection<T> viewItems, ObservableCollection<SearcherOption<T>> options, Func<T, string, bool> isMatch)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.ViewItems = viewItems;
			this.Options = options;
			this.IsMatch = isMatch;
		}

		public List<T> FindAll(SearcherOption<T> option, string search, ObservableCollection<T> viewItems = null)
		{
			if (option == null) throw new ArgumentNullException(nameof(option));
			if (search == null) throw new ArgumentNullException(nameof(search));

			this.CurrentOption = option;
			this.Search = search;

			var items = viewItems ?? this.ViewItems;

			// Find items matching the search
			var searchMatches = new List<T>();
			if (search.Length > 0)
			{
				foreach (var viewItem in items)
				{
					if (this.IsMatch(viewItem, search))
					{
						searchMatches.Add(viewItem);
					}
				}

				items = new ObservableCollection<T>(searchMatches);
			}


			var matches = new List<T>();
			foreach (var viewItem in items)
			{
				if (option.IsMatch(viewItem) && this.IsMatch(viewItem, search))
				{
					matches.Add(viewItem);
				}
			}

			this.SetupCounts(items);

			return matches;
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

		public bool IsChanged(SearcherOption<T> searcherOption = null, string search = null)
		{
			return this.CurrentOption != searcherOption ||
				   this.Search != (search ?? string.Empty);
		}
	}
}