using Cchbc.App.Articles.Objects;
using Cchbc.App.Articles.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.App.Articles
{
	public sealed class ArticlesModule : Module<Article, ArticleViewItem>
	{
		public ArticlesModule(
			Sorter<ArticleViewItem> sorter, 
			Searcher<ArticleViewItem> searcher, 
			FilterOption<ArticleViewItem>[] filterOptions = null) 
			: base(sorter, searcher, filterOptions)
		{
		}
	}
}