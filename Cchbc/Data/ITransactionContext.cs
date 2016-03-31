﻿using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public interface ITransactionContext : IDisposable
	{
		int Execute(Query query, QueryParameter[] parameters = null);

		List<T> Execute<T>(Query<T> query, QueryParameter[] parameters = null);

		void Fill<T>(Dictionary<long, T> items, Func<T, long> selector, Query<T> query);

		void Fill<T>(Dictionary<long, T> items, Action<IFieldDataReader, Dictionary<long, T>> filler, Query query, QueryParameter[] parameters = null);

		long GetNewId();

		void Complete();
	}
}