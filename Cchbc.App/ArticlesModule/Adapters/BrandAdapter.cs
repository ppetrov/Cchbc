using System;
using System.Collections.Generic;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Data;

namespace Cchbc.App.ArticlesModule.Adapters
{
	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private readonly ReadQueryHelper _queryHelper;

		public BrandAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public void Fill(Dictionary<long, Brand> items, Func<Brand, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			_queryHelper.Fill(new Query<Brand>(@"SELECT ID, NAME FROM BRANDS", r =>
			{
				var id = r.GetInt64(0);
				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}
				return new Brand(id, name);
			}), items, selector);
		}
	}
}