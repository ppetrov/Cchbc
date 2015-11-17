using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cchbc.Data
{
	public abstract class ReadQueryHelper
	{
		public abstract Task<List<T>> ExecuteAsync<T>(Query<T> query);

		public abstract Task FillAsync<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector);
	}
}