using System;
using System.Collections.Generic;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Data;

namespace Cchbc.App.ArticlesModule.Adapters
{
	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		public void Fill(ITransactionContext context, Dictionary<long, Brand> items, Func<Brand, long> selector)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			context.Fill(items, selector, new Query<Brand>(@"SELECT ID, NAME FROM BRANDS", r =>
			{
				var id = r.GetInt64(0);
				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}
				return new Brand(id, name);
			}));
		}
	}
}