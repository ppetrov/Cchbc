using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.App.Articles.Objects;
using Cchbc.Data;

namespace Cchbc.App.Articles.Data
{
	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private readonly ReadDataQueryHelper _queryHelper;

		public FlavorAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public async Task FillAsync(Dictionary<long, Flavor> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			await _queryHelper.FillAsync(new Query<Flavor>(@"SELECT ID, NAME FROM FLAVORS", r =>
			{
				var id = r.GetInt64(0);
				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}
				return new Flavor(id, name);
			}), items);
		}
	}
}