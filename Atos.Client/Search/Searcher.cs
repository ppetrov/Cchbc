using System;
using System.Collections.Generic;

namespace Atos.Client.Search
{
	public sealed class Searcher<T> where T : ViewModel
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

		public IEnumerable<T> Search(ICollection<T> viewModels, string textSearch, SearchOption<T> option)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
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

			IEnumerable<T> currentViewModels;

			if (textSearch == string.Empty)
			{
				this.SetupCounts(viewModels);
				currentViewModels = viewModels;
			}
			else
			{
				var filteredByTextViewModels = new List<T>();

				foreach (var item in viewModels)
				{
					if (this.TextMatch(item, textSearch))
					{
						filteredByTextViewModels.Add(item);
					}
				}

				this.SetupCounts(filteredByTextViewModels);
				currentViewModels = filteredByTextViewModels;
			}

			if (option == null)
			{
				foreach (var item in currentViewModels)
				{
					yield return item;
				}
				yield break;
			}
			foreach (var item in currentViewModels)
			{
				if (option.IsMatch(item))
				{
					yield return item;
				}
			}
		}

		public void SetupCounts(ICollection<T> models)
		{
			if (models == null) throw new ArgumentNullException(nameof(models));

			foreach (var option in this.Options)
			{
				var count = 0;
				foreach (var item in models)
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