﻿using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Helpers
{
	public interface IHelper<T>
	{
		Dictionary<long, T> Items { get; }

		void Load(ITransactionContext context, IReadOnlyAdapter<T> adapter, Func<T, long> selector);
	}
}