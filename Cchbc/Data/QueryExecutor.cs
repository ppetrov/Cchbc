using System;
using System.Collections.Generic;
using System.Linq;

namespace Cchbc.Data
{
	public sealed class QueryExecutor
	{
		public ReadQueryExecutor ReadQueryExecutor { get; }
		public ModifyQueryExecutor ModifyQueryExecutor { get; }

		public QueryExecutor(ReadQueryExecutor readQueryExecutor, ModifyQueryExecutor modifyQueryExecutor)
		{
			if (readQueryExecutor == null) throw new ArgumentNullException(nameof(readQueryExecutor));
			if (modifyQueryExecutor == null) throw new ArgumentNullException(nameof(modifyQueryExecutor));

			this.ReadQueryExecutor = readQueryExecutor;
			this.ModifyQueryExecutor = modifyQueryExecutor;
		}

		public int Execute(string statement)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));

			return this.ModifyQueryExecutor.Execute(statement, Enumerable.Empty<QueryParameter>().ToArray());
		}

		public int Execute(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			return this.ModifyQueryExecutor.Execute(statement, parameters);
		}

		public List<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.ReadQueryExecutor.Execute(query);
		}

		public void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			this.ReadQueryExecutor.Fill(query, values, selector);
		}

		public void Fill<T>(string query, Dictionary<long, T> values, Action<IFieldDataReader, Dictionary<long, T>> filler, QueryParameter[] parameters = null)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (filler == null) throw new ArgumentNullException(nameof(filler));

			this.ReadQueryExecutor.Fill(query, values, filler, parameters ?? Enumerable.Empty<QueryParameter>().ToArray());
		}

		public long GetNewId()
		{
			return this.Execute(ModifyQueryExecutor.SelectNewIdQuery)[0];
		}
	}
}