using System;
using System.Collections.Generic;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Data;

namespace Cchbc.App.ArticlesModule.Adapters
{
	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private readonly ReadQueryExecutor _queryExecutor;

		public FlavorAdapter(ReadQueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			_queryExecutor = queryExecutor;
		}

		public void Fill(Dictionary<long, Flavor> items, Func<Flavor, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			_queryExecutor.Fill(new Query<Flavor>(@"SELECT ID, NAME FROM FLAVORS", r =>
			{
				var id = r.GetInt64(0);
				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}
				return new Flavor(id, name);
			}), items, selector);
		}
	}
}