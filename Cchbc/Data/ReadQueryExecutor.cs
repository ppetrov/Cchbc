using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public abstract class ReadQueryExecutor
	{
		public abstract List<T> Execute<T>(Query<T> query);

		public abstract void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector);

		public abstract void Fill<T>(string query, Dictionary<long, T> values, Action<IFieldDataReader, Dictionary<long, T>> selector, QueryParameter[] parameters);
	}
}