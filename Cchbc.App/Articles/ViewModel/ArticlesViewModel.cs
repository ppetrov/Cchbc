using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.App.Articles.Data;
using Cchbc.App.Articles.Helpers;
using Cchbc.App.Articles.Objects;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.App.Articles.ViewModel
{
	public sealed class ArticlesViewModel : ViewObject
	{
		private Core Core { get; }
		private FeatureManager FeatureManager => this.Core.FeatureManager;

		private Module<Article, ArticleViewItem> Module { get; }
		private string Context { get; } = nameof(ArticlesViewModel);

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

				var feature = this.FeatureManager.StartNew(this.Context, @"Search by text", value);
				this.ApplySearch();
				this.FeatureManager.Stop(feature);
			}
		}

		private SearchOption<ArticleViewItem> _searchOption;
		public SearchOption<ArticleViewItem> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);

				var feature = this.FeatureManager.StartNew(this.Context, @"Search by option", value.Name);
				this.ApplySearch();
				this.FeatureManager.Stop(feature);
			}
		}

		private SortOption<ArticleViewItem> _sortOption;
		public SortOption<ArticleViewItem> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				var feature = this.FeatureManager.StartNew(this.Context, @"Sort");
				this.ApplySort();
				this.FeatureManager.Stop(feature);
			}
		}

		public ArticlesViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			this.Core = core;
			this.Module = new ArticlesModule(
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
			var queryHelper = this.Core.QueryHelper.ReadDataQueryHelper;
			var feature = this.FeatureManager.StartNew(this.Context, @"Load Data");

			var step = feature.AddStep(@"Load Brands");
			var brandHelper = new BrandHelper();
			await brandHelper.LoadAsync(new BrandAdapter(queryHelper));
			step.Details = brandHelper.Items.Count.ToString();

			step = feature.AddStep(@"Load Flavors");
			var flavorHelper = new FlavorHelper();
			await flavorHelper.LoadAsync(new FlavorAdapter(queryHelper));
			step.Details = flavorHelper.Items.Count.ToString();

			feature.AddStep(@"Load Articles");
			var articleHelper = new ArticleHelper();
			await articleHelper.LoadAsync(new ArticleAdapter(queryHelper, brandHelper.Items, flavorHelper.Items));

			feature.AddStep(@"Display Articles");
			var index = 0;
			var items = new ArticleViewItem[articleHelper.Items.Count];
			foreach (var item in articleHelper.Items.Values)
			{
				items[index++] = new ArticleViewItem(item);
			}
			this.Module.LoadData(items);
			this.ApplySearch();

			this.FeatureManager.Stop(feature);
		}

		public void ExcludeSuppressed()
		{
			var feature = this.FeatureManager.StartNew(this.Context, @"Exclude suppressed");
			this.Module.FilterOptions[0].Flip();
			this.ApplySearch();
			this.FeatureManager.Stop(feature);
		}

		public void ExcludeNotInTerritory()
		{
			var feature = this.FeatureManager.StartNew(this.Context, @"Exclude not in territory");
			this.Module.FilterOptions[1].Flip();
			this.ApplySearch();
			this.FeatureManager.Stop(feature);
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