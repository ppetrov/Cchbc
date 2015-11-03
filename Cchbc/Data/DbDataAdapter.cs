using System;

namespace Cchbc.Data
{
	public sealed class DbDataAdapter
	{
		public QueryHelper QueryHelper { get; }

		public DbDataAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}
	}
}