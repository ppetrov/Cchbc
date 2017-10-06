using System;
using System.Collections.Generic;

namespace Atos.Client
{
	public sealed class ServiceLocator
	{
		private Dictionary<string, object> Services { get; } = new Dictionary<string, object>();
		private HashSet<string> Creators { get; } = new HashSet<string>();

		public void RegisterService<T>(T service) where T : class
		{
			this.Services.Add(typeof(T).FullName, service);
		}

		public void RegisterServiceCreator<T>(Func<T> serviceCreator) where T : class
		{
			var key = typeof(T).FullName;
			this.Services.Add(key, serviceCreator);
			this.Creators.Add(key);
		}

		public T GetService<T>() where T : class
		{
			var key = typeof(T).FullName;
			var value = this.Services[key];
			if (this.Creators.Contains(key))
			{
				return ((Func<T>)value)();
			}
			return (T)value;
		}
	}
}