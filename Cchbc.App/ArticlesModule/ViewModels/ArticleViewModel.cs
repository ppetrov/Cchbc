using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Objects;

namespace Cchbc.App.ArticlesModule.ViewModels
{
	public sealed class ArticleViewModel : ViewModel<Article>
	{
		public string Name { get; }
		public string Brand { get; }
		public string Flavor { get; }

		public ArticleViewModel(Article article) : base(article)
		{
			this.Name = article.Name;
			this.Brand = article.Brand.Name;
			this.Flavor = article.Flavor.Name;
		}
	}
}