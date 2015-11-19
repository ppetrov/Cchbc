using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public abstract class ModifyQueryHelper
	{
		private ReadQueryHelper ReadQueryHelper { get; }

		protected ModifyQueryHelper(ReadQueryHelper readQueryHelper)
		{
			if (readQueryHelper == null) throw new ArgumentNullException(nameof(readQueryHelper));

			this.ReadQueryHelper = readQueryHelper;
		}

		public List<T> Execute<T>(Query<T> query) where T : IDbObject
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

		public abstract int Execute(string statement, QueryParameter[] parameters);
	}
}