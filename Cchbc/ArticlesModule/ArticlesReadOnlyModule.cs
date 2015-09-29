using System;
using System.Threading.Tasks;
using Cchbc.ArticlesModule.ViewModel;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.ArticlesModule
{
	public sealed class ArticlesReadOnlyModule : ReadOnlyModule<ArticleViewModel>
	{
		public ArticlesReadOnlyModule(ILogger logger, Func<ILogger, Task<ArticleViewModel[]>> dataLoader,
			Sorter<ArticleViewModel> sorter, Searcher<ArticleViewModel> searcher) : base(logger, dataLoader, sorter, searcher)
		{
		}
	}
}