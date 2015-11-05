using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public sealed class QueryHelper
	{
		public ReadDataQueryHelper ReadDataQueryHelper { get; }
		public ModifyDataQueryHelper ModifyDataQueryHelper { get; }

		public QueryHelper(ReadDataQueryHelper readDataQueryHelper, ModifyDataQueryHelper modifyDataQueryHelper)
		{
			//if (readDataQueryHelper == null) throw new ArgumentNullException(nameof(readDataQueryHelper));
			//if (modifyDataQueryHelper == null) throw new ArgumentNullException(nameof(modifyDataQueryHelper));

			this.ReadDataQueryHelper = readDataQueryHelper;
			this.ModifyDataQueryHelper = modifyDataQueryHelper;
		}

		public Task ExecuteAsync(string statement)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));

			return this.ModifyDataQueryHelper.ExecuteAsync(statement, Enumerable.Empty<QueryParameter>().ToArray());
		}

		public Task ExecuteAsync(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			return this.ModifyDataQueryHelper.ExecuteAsync(statement, parameters);
		}

		public Task<List<T>> ExecuteAsync<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.ReadDataQueryHelper.ExecuteAsync(query);
		}

		public Task FillAsync<T>(Query<T> query, Dictionary<long, T> values) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));

			return this.ReadDataQueryHelper.FillAsync(query, values);
		}
	}
}