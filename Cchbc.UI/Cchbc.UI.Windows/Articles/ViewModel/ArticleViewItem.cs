using Cchbc.Objects;

namespace Cchbc.ArticlesModule.ViewModel
{
	public sealed class ArticleViewItem : ViewItem<Article>
	{
		public long Id { get; }
		public string Name { get; }
		public string Brand { get; }
		public string Flavor { get; }
		public decimal Quantity { get; }

		public ArticleViewItem(Article article) : base(article)
		{
			this.Id = article.Id;
			this.Name = article.Name;
			this.Brand = article.Brand.Name;
			this.Flavor = article.Flavor.Name;
		}
	}
}