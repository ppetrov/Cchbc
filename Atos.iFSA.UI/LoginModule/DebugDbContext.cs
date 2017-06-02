using System.Collections.Generic;
using System.Diagnostics;
using Atos.Client.Data;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed class DebugDbContext : IDbContext
	{
		public void Dispose()
		{
			Debug.WriteLine(@"Dispose DbContext");
		}

		public int Execute(Query query)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<T> Execute<T>(Query<T> query)
		{
			throw new System.NotImplementedException();
		}

		public void Complete()
		{
			Debug.WriteLine(@"Commit SQL transaction");
		}
	}
}