using System;

namespace Cchbc.Data
{
	public sealed class DataAdapter
	{
		public QueryHelper QueryHelper { get; }

		public DataAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}
	}
}