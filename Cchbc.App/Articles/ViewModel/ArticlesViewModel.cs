using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.App.Articles.Data;
using Cchbc.App.Articles.Helpers;
using Cchbc.App.Articles.Objects;
using Cchbc.Data;
using Cchbc.Features;
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

				var feature = Feature.StartNew(this.Context, nameof(SearchByText));
				this.SearchByText();
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
				var feature = Feature.StartNew(this.Context, nameof(SearchByOption), value?.Name ?? @"N/A");
				this.SearchByOption();
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
				var feature = Feature.StartNew(this.Context, nameof(SortBy));
				this.SortBy();
				this.FeatureManager.Stop(feature);
			}
		}

		private FilterOption<ArticleViewItem> ExcludeSuppressedFilterOption => this.Module.FilterOptions[0];
		private FilterOption<ArticleViewItem> ExcludeNotInTerritoryFilterOption => this.Module.FilterOptions[1];

		public ArticlesViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			// TODO : Customize strings

			this.Core = core;
			this.Module = new ArticlesModule(
				new Sorter<ArticleViewItem>(new[]
				{
					new SortOption<ArticleViewItem>(@"Name", (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal)),
					new SortOption<ArticleViewItem>(@"Brand", (x, y) => string.Compare(x.Brand, y.Brand, StringComparison.Ordinal)),
					new SortOption<ArticleViewItem>(@"Flavor", (x, y) => string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal)),
				}),
				new Searcher<ArticleViewItem>(new[]
				{
					new SearchOption<ArticleViewItem>(@"All", v => true, true),
					new SearchOption<ArticleViewItem>(@"Coca Cola", v => v.Brand[0] == 'C'),
					new SearchOption<ArticleViewItem>(@"Fanta", v => v.Brand[0] == 'F'),
					new SearchOption<ArticleViewItem>(@"Sprite", v => v.Brand[0] == 'S'),
				},
				(item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0), new[]
				{
					new FilterOption<ArticleViewItem>(@"Exclude suppressed", v => v.Name.IndexOf('S') >= 0),
					new FilterOption<ArticleViewItem>(@"Exclude not in territory", v => v.Name.IndexOf('F') >= 0),
				});
		}

		public async Task LoadDataAsync()
		{
			var queryHelper = this.Core.QueryHelper.ReadDataQueryHelper;
			var feature = Feature.StartNew(this.Context, nameof(LoadDataAsync));

			var brandHelper = await LoadBrandsAsync(feature, queryHelper);
			var flavorHelper = await LoadFlavorsAsync(feature, queryHelper);
			var articleHelper = await LoadArticlesAsync(feature, queryHelper, brandHelper, flavorHelper);
			this.DisplayArticles(feature, articleHelper);

			this.FeatureManager.Stop(feature);
		}

		private async Task<BrandHelper> LoadBrandsAsync(Feature feature, ReadDataQueryHelper queryHelper)
		{
			var step = feature.AddStep(nameof(LoadBrandsAsync));
			var brandHelper = new BrandHelper();
			await brandHelper.LoadAsync(new BrandAdapter(queryHelper));
			step.Details = brandHelper.Items.Count.ToString();
			return brandHelper;
		}

		private async Task<FlavorHelper> LoadFlavorsAsync(Feature feature, ReadDataQueryHelper queryHelper)
		{
			var step = feature.AddStep(nameof(LoadFlavorsAsync));
			var flavorHelper = new FlavorHelper();
			await flavorHelper.LoadAsync(new FlavorAdapter(queryHelper));
			step.Details = flavorHelper.Items.Count.ToString();
			return flavorHelper;
		}

		private async Task<ArticleHelper> LoadArticlesAsync(Feature feature, ReadDataQueryHelper queryHelper, BrandHelper brandHelper, FlavorHelper flavorHelper)
		{
			var step = feature.AddStep(nameof(LoadArticlesAsync));
			var articleHelper = new ArticleHelper();
			await articleHelper.LoadAsync(new ArticleAdapter(queryHelper, brandHelper.Items, flavorHelper.Items));
			step.Details = articleHelper.Items.Count.ToString();
			return articleHelper;
		}

		private void DisplayArticles(Feature feature, ArticleHelper articleHelper)
		{
			feature.AddStep(nameof(DisplayArticles));
			this.Module.LoadData(articleHelper.Items.Values.Select(v => new ArticleViewItem(v)).ToArray());
			this.ApplySearch();
		}

		public void ExcludeSuppressed()
		{
			var feature = Feature.StartNew(this.Context, nameof(ExcludeSuppressed));
			this.ExcludeSuppressedFilterOption.Flip();
			this.ApplySearch();
			this.FeatureManager.Stop(feature);
		}

		public void ExcludeNotInTerritory()
		{
			var feature = Feature.StartNew(this.Context, nameof(ExcludeNotInTerritory));
			this.ExcludeNotInTerritoryFilterOption.Flip();
			this.ApplySearch();
			this.FeatureManager.Stop(feature);
		}

		private void SearchByText() => this.ApplySearch();

		private void SearchByOption() => this.ApplySearch();

		private void ApplySearch()
		{
			var viewItems = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Articles.Clear();
			foreach (var viewItem in viewItems)
			{
				this.Articles.Add(viewItem);
			}
		}

		private void SortBy()
		{
			var index = 0;
			foreach (var viewItem in this.Module.Sort(this.Articles, this.SortOption))
			{
				this.Articles[index++] = viewItem;
			}
		}
	}
}