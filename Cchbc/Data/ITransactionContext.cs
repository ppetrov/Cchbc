using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public interface ITransactionContext : IDisposable
	{
		int Execute(Query query);

		List<T> Execute<T>(Query<T> query);

		void Fill<T>(Dictionary<long, T> items, Func<T, long> selector, Query<T> query);

		void Fill<T>(Dictionary<long, T> items, Action<IFieldDataReader, Dictionary<long, T>> filler, Query query);

		long GetNewId();

		void Complete();
	}
}