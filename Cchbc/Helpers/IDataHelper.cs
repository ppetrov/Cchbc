﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.Helpers
{
	public interface IDataHelper<T> where T : IDbObject
	{
		Dictionary<long, T> Items { get; }

		Task LoadAsync(IReadOnlyAdapter<T> adapter);
	}
}