using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public abstract class ReadQueryHelper
	{
		public abstract List<T> Execute<T>(Query<T> query);

		public abstract void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector);
	}
}