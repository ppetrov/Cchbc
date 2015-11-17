using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		public Task<List<T>> ExecuteAsync<T>(Query<T> query) where T : IDbObject
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

		public abstract Task ExecuteAsync(string statement, QueryParameter[] parameters);
	}
}