using Cchbc.App.ArticlesModule.Objects;
using Cchbc.App.ArticlesModule.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.App.ArticlesModule
{
	public sealed class ArticlesReadOnlyManager : ReadOnlyManager<Article, ArticleViewItem>
	{
		public ArticlesReadOnlyManager(
			Sorter<ArticleViewItem> sorter, 
			Searcher<ArticleViewItem> searcher, 
			FilterOption<ArticleViewItem>[] filterOptions = null) 
			: base(sorter, searcher, filterOptions)
		{
		}
	}
}