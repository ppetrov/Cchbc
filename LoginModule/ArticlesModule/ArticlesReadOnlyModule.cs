using Cchbc.App.ArticlesModule.Objects;
using Cchbc.App.ArticlesModule.ViewModels;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.App.ArticlesModule
{
	public sealed class ArticlesReadOnlyModule : ReadOnlyModule<Article, ArticleViewModel>
	{
		public ArticlesReadOnlyModule(
			Sorter<ArticleViewModel> sorter,
			Searcher<ArticleViewModel> searcher,
			FilterOption<ArticleViewModel>[] filterOptions = null)
			: base(sorter, searcher, filterOptions)
		{
		}
	}
}