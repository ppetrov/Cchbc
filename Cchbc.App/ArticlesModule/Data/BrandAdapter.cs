using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Data;

namespace Cchbc.App.ArticlesModule.Data
{
	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private readonly ReadQueryHelper _queryHelper;

		public BrandAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, Brand> items, Func<Brand, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return _queryHelper.FillAsync(new Query<Brand>(@"SELECT ID, NAME FROM BRANDS", r =>
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