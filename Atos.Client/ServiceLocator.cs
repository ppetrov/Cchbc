using System.Collections.Generic;

namespace Atos.Client
{
	public sealed class ServiceLocator
	{
		private Dictionary<string, object> RegisteredServices { get; } = new Dictionary<string, object>();

		public void RegisterService<T>(T service)
		{
			var key = typeof(T).FullName;
			if (!this.RegisteredServices.ContainsKey(key))
			{
				this.RegisteredServices.Add(key, service);
			}
		}

		public T GetService<T>()
		{
			return (T)this.RegisteredServices[typeof(T).FullName];
		}
	}
}