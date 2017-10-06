using System;
using System.Collections.Generic;

namespace Atos.Client.Data
{
	public interface IDbContext : IDisposable
	{
		int Execute(Query query);

		IEnumerable<T> Execute<T>(Query<T> query);

		void Complete();
	}
}