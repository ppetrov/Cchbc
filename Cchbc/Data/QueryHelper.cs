using System;
using System.Collections.Generic;
using System.Linq;
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

		public int Execute(string statement)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));

			return this.ModifyQueryHelper.Execute(statement, Enumerable.Empty<QueryParameter>().ToArray());
		}

		public int Execute(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			return this.ModifyQueryHelper.Execute(statement, parameters);
		}

		public List<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.ReadQueryHelper.Execute(query);
		}

		public void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			this.ReadQueryHelper.Fill(query, values, selector);
		}

		public long GetNewId()
		{
			return this.Execute(ModifyQueryHelper.SelectNewIdQuery)[0];
		}
	}
}