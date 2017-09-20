using System.Collections.Generic;

namespace Atos.Client
{
	public sealed class ServiceLocator : IServiceLocator
	{
		public static ServiceLocator Current { get; } = new ServiceLocator();

		private Dictionary<string, object> RegisteredServices { get; } = new Dictionary<string, object>();

		public T GetService<T>()
		{
			return (T)this.RegisteredServices[typeof(T).FullName];
		}

		public void RegisterService<T>(T service)
		{
			var key = typeof(T).FullName;
			if (!this.RegisteredServices.ContainsKey(key))
			{
				this.RegisteredServices.Add(key, service);
			}
		}
	}
}