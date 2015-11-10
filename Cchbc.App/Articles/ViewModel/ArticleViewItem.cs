using Cchbc.App.Articles.Objects;
using Cchbc.Objects;

namespace Cchbc.App.Articles.ViewModel
{
	public sealed class ArticleViewItem : ViewItem<Article>
	{
		public string Name { get; }
		public string Brand { get; }
		public string Flavor { get; }
		public long TodayQuantity { get; set; }

		public ArticleViewItem(Article article) : base(article)
		{
			this.Name = article.Name;
			this.Brand = article.Brand.Name;
			this.Flavor = article.Flavor.Name;
			this.TodayQuantity = 0;
		}
	}
}