﻿using System;
using System.Collections.Generic;

namespace Cchbc.Data
{
	public interface IReadOnlyAdapter<T>
	{
		void Fill(ITransactionContext context, Dictionary<long, T> items, Func<T, long> selector);
	}
}