using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.ArticlesModule.ViewModel
{
	public sealed class ArticlesViewModel : ViewObject
	{
		private ReadOnlyModule<ArticleViewModel> Module { get; }

		public ObservableCollection<ArticleViewModel> Articles { get; } = new ObservableCollection<ArticleViewModel>();
		public SortOption<ArticleViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<ArticleViewModel>[] SearchOptions => this.Module.Searcher.Options;

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySearch();
			}
		}

		private SearchOption<ArticleViewModel> _searchOption;
		public SearchOption<ArticleViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySearch();
			}
		}

		private SortOption<ArticleViewModel> _sortOption;
		public SortOption<ArticleViewModel> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySort();
			}
		}

		public ArticlesViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Module = new ArticlesReadOnlyModule(logger, CreateDataLoader, CreateSorter(), CreateSearcher());
			this.Module.FilterOptions = CreateFilterOptions();
		}

		private static async Task<ArticleViewModel[]> CreateDataLoader(ILogger logger)
		{
			var brandHelper = new BrandHelper();
			await brandHelper.LoadAsync(new BrandAdapter(logger));

			var flavorHelper = new FlavorHelper();
			await flavorHelper.LoadAsync(new FlavorAdapter(logger));

			var articleHelper = new ArticleHelper();
			await articleHelper.LoadAsync(new ArticleAdapter(logger, brandHelper.Items, flavorHelper.Items));

			return articleHelper.Items.Values.Select(v => new ArticleViewModel(v)).ToArray();
		}

		private static Sorter<ArticleViewModel> CreateSorter()
		{
			return new Sorter<ArticleViewModel>(new[]
			{
				new SortOption<ArticleViewModel>(@"Name", (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal)),
				new SortOption<ArticleViewModel>(@"Brand", (x, y) => string.Compare(x.Brand, y.Brand, StringComparison.Ordinal)),
				new SortOption<ArticleViewModel>(@"Flavor", (x, y) => string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal)),
			});
		}

		private static Searcher<ArticleViewModel> CreateSearcher()
		{
			return new Searcher<ArticleViewModel>(new[]
			{
				new SearchOption<ArticleViewModel>(@"All", v => true, true),
				new SearchOption<ArticleViewModel>(@"Coca Cola", v => v.Brand[0] == 'C'),
				new SearchOption<ArticleViewModel>(@"Fanta", v => v.Brand[0] == 'F'),
				new SearchOption<ArticleViewModel>(@"Sprite", v => v.Brand[0] == 'S'),
			}, (item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static FilterOption<ArticleViewModel>[] CreateFilterOptions()
		{
			return new[]
			{
				new FilterOption<ArticleViewModel>(@"Exclude suppressed", v => v.Name.IndexOf('S') >= 0),
				new FilterOption<ArticleViewModel>(@"Exclude not in territory", v => v.Name.IndexOf('F') >= 0),
			};
		}

		public async Task LoadDataAsync()
		{
			// TODO : Log operation & time
			var s = Stopwatch.StartNew();

			await this.Module.LoadDataAsync();

			//logger.Info(@"Loading articles...");
			//logger.Info($@"Articles loaded in {s.ElapsedMilliseconds} ms");
			//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
			//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");

			this.ApplySearch();
		}

		public void ExcludeSuppressed()
		{
			var s = Stopwatch.StartNew();
			this.Module.Logger.Info(@"Loading articles...");

			// TODO : !!! Log operation & time
			this.Module.FilterOptions.First().Flip();
			this.ApplySearch();

			// TODO : Logger
			// TODO : Statistics to track functionality usage
			//this.Stats[ExcludeSuppressed]++;
		}

		public void ExcludeNotInTerritory()
		{
			// TODO : !!! Log operation & time
			this.Module.FilterOptions.Last().Flip();
			this.ApplySearch();
		}

		private void ApplySearch()
		{
			var viewItems = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Articles.Clear();
			foreach (var viewItem in viewItems)
			{
				this.Articles.Add(viewItem);
			}
		}

		private void ApplySort()
		{
			var index = 0;
			foreach (var viewItem in this.Module.Sort(this.Articles, this.SortOption))
			{
				this.Articles[index++] = viewItem;
			}
		}
	}
}