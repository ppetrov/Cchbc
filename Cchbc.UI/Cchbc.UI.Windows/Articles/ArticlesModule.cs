using Cchbc.ArticlesModule.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI.Articles
{
	public sealed class ArticlesModule : Module<ArticleViewModel>
	{
		public ArticlesModule(
			Sorter<ArticleViewModel> sorter,
			Searcher<ArticleViewModel> searcher,
			FilterOption<ArticleViewModel>[] filterOptions = null) :
			base(sorter, searcher, filterOptions)
		{
		}
	}
}