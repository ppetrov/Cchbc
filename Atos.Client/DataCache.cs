using System;
using System.Collections.Generic;
using Atos.Client.Data;

namespace Atos.Client
{
	public sealed class DataCache
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
		private readonly Dictionary<string, object> _dataProviders = new Dictionary<string, object>();

		public void Register<T>(Func<IDbContext, DataCache, Dictionary<long, T>> dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			_dataProviders.Add(typeof(T).FullName, dataProvider);
		}

		public void Unregister<T>()
		{
			_dataProviders.Remove(typeof(T).FullName);
		}

		public Dictionary<long, T> GetValues<T>(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var key = typeof(T).FullName;

			object value;
			if (!_values.TryGetValue(key, out value))
			{
				var dataProvider = (Func<IDbContext, DataCache, Dictionary<long, T>>)_dataProviders[key];
				value = dataProvider(context, this);
				_values.Add(key, value);
			}

			return (Dictionary<long, T>)value;
		}

		public void RemoveValues<T>()
		{
			_values.Remove(typeof(T).FullName);
		}

		public void Clear()
		{
			_values.Clear();
		}
	}
}