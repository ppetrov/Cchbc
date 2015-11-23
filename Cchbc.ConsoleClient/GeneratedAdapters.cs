using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{



	public sealed class OutletAdapter : IReadOnlyAdapter<Outlet>
	{
		private QueryHelper QueryHelper { get; }

		public OutletAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public void Fill(Dictionary<long, Outlet> items, Func<Outlet, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name FROM Outlets";

			this.QueryHelper.Fill(new Query<Outlet>(query, OutletCreator), items, selector);
		}

		private static Outlet OutletCreator(IFieldDataReader r)
		{
			var id = 0L;
			if (!r.IsDbNull(0))
			{
				id = r.GetInt64(0);
			}

			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}

			return new Outlet(id, name);
		}
    }


































}