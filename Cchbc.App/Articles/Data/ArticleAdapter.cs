using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.App.Articles.Objects;
using Cchbc.Data;

namespace Cchbc.App.Articles.Data
{
	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private readonly ReadDataQueryHelper _queryHelper;
		private readonly Dictionary<long, Brand> _brands;
		private readonly Dictionary<long, Flavor> _flavors;

		public ArticleAdapter(ReadDataQueryHelper queryHelper, Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (brands == null) throw new ArgumentNullException(nameof(brands));
			if (flavors == null) throw new ArgumentNullException(nameof(flavors));

			_queryHelper = queryHelper;
			_brands = brands;
			_flavors = flavors;
		}

		public Task FillAsync(Dictionary<long, Article> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			//items.Add(1, new Article(1, @"Coca Cola 2.0L PET", Brands[1], Flavors[1]));
			//items.Add(2, new Article(2, @"Fanta 2.0L PET", Brands[2], Flavors[2]));
			//items.Add(3, new Article(3, @"Sprite 2.0L PET", Brands[3], Flavor.Empty));

			return Task.FromResult(true);
		}
	}
}