using System;
using System.Collections.ObjectModel;
using System.Linq;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.App.ArticlesModule.ViewModels
{
	public sealed class ArticlesViewModel : ViewModel
	{
		private Core Core { get; }
		private FeatureManager FeatureManager => this.Core.FeatureManager;
		private ReadOnlyModule<Article, ArticleViewModel> Module { get; }
		private string Context { get; } = nameof(ArticlesViewModel);

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

				var feature = Feature.StartNew(this.Context, nameof(SearchByText));
				this.SearchByText();
				this.FeatureManager.Stop(feature);
			}
		}

		private SearchOption<ArticleViewModel> _searchOption;
		public SearchOption<ArticleViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				var feature = Feature.StartNew(this.Context, nameof(SearchByOption), value?.Name ?? string.Empty);
				this.SearchByOption();
				this.FeatureManager.Stop(feature);
			}
		}

		private SortOption<ArticleViewModel> _sortOption;
		public SortOption<ArticleViewModel> SortOption
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

		public ArticlesViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			this.Core = core;
			var local = core.LocalizationManager;
			var sorter = new Sorter<ArticleViewModel>(new[]
			{
				new SortOption<ArticleViewModel>(local[LocalizationKeys.Name], (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal)),
				new SortOption<ArticleViewModel>(local[LocalizationKeys.Brand], (x, y) => string.Compare(x.Brand, y.Brand, StringComparison.Ordinal)),
				new SortOption<ArticleViewModel>(local[LocalizationKeys.Flavor], (x, y) => string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal)),
			});
			var searcher = new Searcher<ArticleViewModel>(new[]
			{
				new SearchOption<ArticleViewModel>(local[LocalizationKeys.All], v => true, true),
			}, (item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

			this.Module = new ArticlesReadOnlyModule(sorter, searcher);
		}

		public void LoadData()
		{
			var feature = Feature.StartNew(this.Context, nameof(LoadData));

			var articlesHelper = this.Core.DataCache.GetHelper<Article>();
			var viewModels = articlesHelper.Items.Values.Select(v => new ArticleViewModel(v)).ToArray();
			this.DisplayArticles(feature, viewModels);

			this.FeatureManager.Stop(feature);
		}

		private void DisplayArticles(Feature feature, ArticleViewModel[] viewModels)
		{
			feature.AddStep(nameof(DisplayArticles));
			this.Module.SetupViewModels(viewModels);
			this.ApplySearch();
		}

		private void SearchByText() => this.ApplySearch();

		private void SearchByOption() => this.ApplySearch();

		private void ApplySearch()
		{
			var viewModels = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Articles.Clear();
			foreach (var viewModel in viewModels)
			{
				this.Articles.Add(viewModel);
			}
		}

		private void SortBy()
		{
			var index = 0;
			foreach (var viewModel in this.Module.Sort(this.Articles, this.SortOption))
			{
				this.Articles[index++] = viewModel;
			}
		}
	}
}