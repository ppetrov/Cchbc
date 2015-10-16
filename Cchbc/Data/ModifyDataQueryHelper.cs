using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public abstract class ModifyDataQueryHelper
	{
		private ReadDataQueryHelper ReadDataQueryHelper { get; }

		protected ModifyDataQueryHelper(ReadDataQueryHelper readDataQueryHelper)
		{
			if (readDataQueryHelper == null) throw new ArgumentNullException(nameof(readDataQueryHelper));

			this.ReadDataQueryHelper = readDataQueryHelper;
		}

		public Task<List<T>> ExecuteAsync<T>(Query<T> query) where T : IDbObject
		{
			return this.ReadDataQueryHelper.ExecuteAsync(query);
		}

		public Task FillAsync<T>(Query<T> query, Dictionary<long, T> values) where T : IDbObject
		{
			return this.ReadDataQueryHelper.FillAsync(query, values);
		}

		public abstract Task ExecuteAsync(string statement, QueryParameter[] parameters);
	}
}