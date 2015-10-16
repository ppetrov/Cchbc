﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public abstract class ReadDataQueryHelper
	{
		public abstract Task<List<T>> ExecuteAsync<T>(Query<T> query) where T : IDbObject;

		public abstract Task FillAsync<T>(Query<T> query, Dictionary<long, T> values) where T : IDbObject;
	}
}