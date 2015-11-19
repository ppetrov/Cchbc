using System;
using System.Collections.Generic;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Data;

namespace Cchbc.App.ArticlesModule.Data
{
	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private readonly ReadQueryHelper _queryHelper;
		private readonly Dictionary<long, Brand> _brands;
		private readonly Dictionary<long, Flavor> _flavors;

		public ArticleAdapter(ReadQueryHelper queryHelper, Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (brands == null) throw new ArgumentNullException(nameof(brands));
			if (flavors == null) throw new ArgumentNullException(nameof(flavors));

			_queryHelper = queryHelper;
			_brands = brands;
			_flavors = flavors;
		}

		public void Fill(Dictionary<long, Article> items, Func<Article, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			_queryHelper.Fill(new Query<Article>(@"SELECT ID, NAME, BRAND_ID, FLAVOR_ID FROM ARTICLES", r =>
			{
				var id = r.GetInt64(0);
				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}
				var brand = Brand.Empty;
				if (!r.IsDbNull(2))
				{
					brand = _brands[r.GetInt64(2)];
				}
				var flavor = Flavor.Empty;
				if (!r.IsDbNull(3))
				{
					flavor = _flavors[r.GetInt64(3)];
				}
				return new Article(id, name, brand, flavor);
			}), items, selector);
		}
	}
}