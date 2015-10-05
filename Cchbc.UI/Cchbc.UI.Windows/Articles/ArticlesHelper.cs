using Cchbc.ArticlesModule;
using Cchbc.ArticlesModule.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI.Articles
{
	public sealed class ArticlesHelper : Helper<Article, ArticleViewItem>
	{
		public ArticlesHelper(
			Sorter<ArticleViewItem> sorter, 
			Searcher<ArticleViewItem> searcher, 
			FilterOption<ArticleViewItem>[] filterOptions = null) 
			: base(sorter, searcher, filterOptions)
		{
		}
	}
}