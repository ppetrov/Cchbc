using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public interface IDbContext : IDisposable
	{
		int Execute(Query query);

		IEnumerable<T> Execute<T>(Query<T> query);

		void Fill<TK, TV>(Dictionary<TK, TV> items, Action<IFieldDataReader, Dictionary<TK, TV>> filler, Query query);

		void Complete();
	}
}