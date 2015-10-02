using System;
using Cchbc.Objects;

namespace Cchbc.ArticlesModule.ViewModel
{
	public sealed class ArticleViewModel : IReadOnlyObject
	{
		public long Id { get; }
		public string Name { get; }
		public string Brand { get; }
		public string Flavor { get; }

		public ArticleViewModel(Article article)
		{
			if (article == null) throw new ArgumentNullException(nameof(article));

			this.Id = article.Id;
			this.Name = article.Name;
			this.Brand = article.Brand.Name;
			this.Flavor = article.Flavor.Name;
		}
	}
}