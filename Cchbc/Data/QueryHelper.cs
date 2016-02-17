using System;
using System.Collections.Generic;
using System.Linq;

namespace Cchbc.Data
{
	public sealed class QueryHelper
	{
		public ReadQueryHelper ReadQueryHelper { get; set; }
		public ModifyQueryHelper ModifyQueryHelper { get; set; }

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

		public void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			this.ReadQueryHelper.Fill(query, values, selector);
		}

		public void Fill<T>(string query, Dictionary<long, T> values, Action<IFieldDataReader, Dictionary<long, T>> filler, QueryParameter[] parameters = null)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (filler == null) throw new ArgumentNullException(nameof(filler));

			this.ReadQueryHelper.Fill(query, values, filler, parameters ?? Enumerable.Empty<QueryParameter>().ToArray());
		}

		public long GetNewId()
		{
			return this.Execute(ModifyQueryHelper.SelectNewIdQuery)[0];
		}
	}
}