﻿using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc
{
	public sealed class DataCache
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
		private readonly Dictionary<string, object> _dataProviders = new Dictionary<string, object>();

		public void Register<T>(Func<ITransactionContext, DataCache, Dictionary<long, T>> dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			_dataProviders.Add(typeof(T).Name, dataProvider);
		}

		public void Unregister<T>()
		{
			_dataProviders.Remove(typeof(T).Name);
		}

		public Dictionary<long, T> GetValues<T>(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var key = typeof(T).Name;

			object value;
			if (!_values.TryGetValue(key, out value))
			{
				var dataProvider = (Func<ITransactionContext, DataCache, Dictionary<long, T>>)_dataProviders[key];
				_values.Add(key, dataProvider(context, this));
			}

			return (Dictionary<long, T>)value;
		}

		public void RemoveValues<T>()
		{
			_values.Remove(typeof(T).Name);
		}

		public void Clear()
		{
			_values.Clear();
		}
	}
}