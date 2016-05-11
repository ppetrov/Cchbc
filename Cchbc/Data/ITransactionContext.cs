using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public interface ITransactionContext : IDisposable
	{
		int Execute(Query query);

		List<T> Execute<T>(Query<T> query);

		void Fill<T>(Dictionary<long, T> items, Func<T, long> selector, Query<T> query);

		void Fill<TK, TV>(Dictionary<TK, TV> items, Action<IFieldDataReader, Dictionary<TK, TV>> filler, Query query);

		long GetNewId();

		void Complete();
	}
}