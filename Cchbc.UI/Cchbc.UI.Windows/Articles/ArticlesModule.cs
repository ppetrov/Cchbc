using Cchbc.ArticlesModule;
using Cchbc.ArticlesModule.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI.Articles
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