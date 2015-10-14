using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.ArticlesModule;
using Cchbc.ArticlesModule.ViewModel;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.UI.Articles;

namespace Cchbc.UI.ArticlesModule.ViewModel
{
	public sealed class ArticlesViewModel : ViewObject
	{
		private ILogger Logger { get; }
		private Module<Article, ArticleViewItem> Module { get; }

		public ObservableCollection<ArticleViewItem> Articles { get; } = new ObservableCollection<ArticleViewItem>();
		public SortOption<ArticleViewItem>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<ArticleViewItem>[] SearchOptions => this.Module.Searcher.Options;

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

		private SearchOption<ArticleViewItem> _searchOption;
		public SearchOption<ArticleViewItem> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
				//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySearch();
			}
		}

		private SortOption<ArticleViewItem> _sortOption;
		public SortOption<ArticleViewItem> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
				//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySort();
			}
		}

		public ArticlesViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
			this.Module = new Articles.ArticlesModule(
				new Sorter<ArticleViewItem>(new[]
				{
					new SortOption<ArticleViewItem>(@"Name", (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal)),
					new SortOption<ArticleViewItem>(@"Brand", (x, y) => string.Compare(x.Brand, y.Brand, StringComparison.Ordinal)),
					new SortOption<ArticleViewItem>(@"Flavor", (x, y) => string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal)),
				}), new Searcher<ArticleViewItem>(new[]
				{
					new SearchOption<ArticleViewItem>(@"All", v => true, true),
					new SearchOption<ArticleViewItem>(@"Coca Cola", v => v.Brand[0] == 'C'),
					new SearchOption<ArticleViewItem>(@"Fanta", v => v.Brand[0] == 'F'),
					new SearchOption<ArticleViewItem>(@"Sprite", v => v.Brand[0] == 'S'),
				}, (item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0), new[]
				{
					new FilterOption<ArticleViewItem>(@"Exclude suppressed", v => v.Name.IndexOf('S') >= 0),
					new FilterOption<ArticleViewItem>(@"Exclude not in territory", v => v.Name.IndexOf('F') >= 0),
				});
		}

		public async Task LoadDataAsync()
		{
			// TODO : Log operation & time
			var s = Stopwatch.StartNew();
			this.Logger.Info(@"Loading articles...");

			var brandHelper = new BrandHelper();
			await brandHelper.LoadAsync(new BrandAdapter(this.Logger));

			var flavorHelper = new FlavorHelper();
			await flavorHelper.LoadAsync(new FlavorAdapter(this.Logger));

			var articleHelper = new ArticleHelper();
			await articleHelper.LoadAsync(new ArticleAdapter(this.Logger, brandHelper.Items, flavorHelper.Items));

			var index = 0;
			var items = new ArticleViewItem[articleHelper.Items.Count];
			foreach (var item in articleHelper.Items.Values)
			{
				items[index++] = new ArticleViewItem(item);
			}
			this.Module.LoadData(items);

			this.ApplySearch();
			this.Logger.Info($@"Articles loaded in {s.ElapsedMilliseconds} ms");
		}

		public void ExcludeSuppressed()
		{
			//var s = Stopwatch.StartNew();
			//this.Logger.Info(@"Loading articles...");

			// TODO : !!! Log operation & time
			this.Module.FilterOptions[0].Flip();
			this.ApplySearch();

			// TODO : Logger
			// TODO : Statistics to track functionality usage
			//this.Stats[ExcludeSuppressed]++;
		}

		public void ExcludeNotInTerritory()
		{
			// TODO : !!! Log operation & time
			this.Module.FilterOptions[1].Flip();
			this.ApplySearch();
		}

		private void ApplySearch()
		{
			//var viewItems = this.ReadOnlyManager.Search(this.TextSearch, this.SearchOption);

			//this.Articles.Clear();
			//foreach (var viewItem in viewItems)
			//{
			//	this.Articles.Add(viewItem);
			//}
		}

		private void ApplySort()
		{
			//var index = 0;
			//foreach (var viewItem in this.ReadOnlyManager.Sort(this.Articles, this.SortOption))
			//{
			//	this.Articles[index++] = viewItem;
			//}
		}
	}
}