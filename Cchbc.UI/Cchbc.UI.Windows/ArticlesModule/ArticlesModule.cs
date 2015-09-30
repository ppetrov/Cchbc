using System;
using System.Threading.Tasks;
using Cchbc.ArticlesModule.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI.ArticlesModule
{
	public sealed class ArticlesModule : Module<ArticleViewModel>
	{
		public ArticlesModule(ILogger logger, Func<ILogger, Task<ArticleViewModel[]>> dataLoader,
			Sorter<ArticleViewModel> sorter, Searcher<ArticleViewModel> searcher) : base(logger, dataLoader, sorter, searcher)
		{
		}
	}
}