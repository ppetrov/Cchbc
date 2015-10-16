using System;
using System.Collections.Generic;
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
			if (readDataQueryHelper == null) throw new ArgumentNullException(nameof(readDataQueryHelper));
			if (modifyDataQueryHelper == null) throw new ArgumentNullException(nameof(modifyDataQueryHelper));

			this.ReadDataQueryHelper = readDataQueryHelper;
			this.ModifyDataQueryHelper = modifyDataQueryHelper;
		}

		public void ExecuteAsync(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			this.ModifyDataQueryHelper.ExecuteAsync(statement, parameters);
		}

		public Task<List<T>> ExecuteAsync<T>(Query<T> query) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.ReadDataQueryHelper.ExecuteAsync(query);
		}

		public void Fill<T>(Query<T> query, Dictionary<long, T> values) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));

			this.ReadDataQueryHelper.FillAsync(query, values);
		}
	}
}