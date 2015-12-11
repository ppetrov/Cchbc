using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public abstract class ModifyQueryExecutor
	{
		public static readonly Query<long> SelectNewIdQuery = new Query<long>(@"SELECT LAST_INSERT_ROWID()", r => r.GetInt64(0));

		private ReadQueryExecutor ReadQueryExecutor { get; }

		protected ModifyQueryExecutor(ReadQueryExecutor readQueryExecutor)
		{
			if (readQueryExecutor == null) throw new ArgumentNullException(nameof(readQueryExecutor));

			this.ReadQueryExecutor = readQueryExecutor;
		}

		public List<T> Execute<T>(Query<T> query) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.ReadQueryExecutor.Execute(query);
		}

		public void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			this.ReadQueryExecutor.Fill(query, values, selector);
		}

		public abstract int Execute(string statement, QueryParameter[] parameters);
	}
}