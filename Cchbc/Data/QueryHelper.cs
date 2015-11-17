using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public sealed class QueryHelper
	{
		public ReadQueryHelper ReadQueryHelper { get; }
		public ModifyQueryHelper ModifyQueryHelper { get; }

		public QueryHelper(ReadQueryHelper readQueryHelper, ModifyQueryHelper modifyQueryHelper)
		{
			if (readQueryHelper == null) throw new ArgumentNullException(nameof(readQueryHelper));
			if (modifyQueryHelper == null) throw new ArgumentNullException(nameof(modifyQueryHelper));

			this.ReadQueryHelper = readQueryHelper;
			this.ModifyQueryHelper = modifyQueryHelper;
		}

		public Task ExecuteAsync(string statement)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));

			return this.ModifyQueryHelper.ExecuteAsync(statement, Enumerable.Empty<QueryParameter>().ToArray());
		}

		public Task ExecuteAsync(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			return this.ModifyQueryHelper.ExecuteAsync(statement, parameters);
		}

		public Task<List<T>> ExecuteAsync<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.ReadQueryHelper.ExecuteAsync(query);
		}

		public Task FillAsync<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return this.ReadQueryHelper.FillAsync(query, values, selector);
		}
	}
}