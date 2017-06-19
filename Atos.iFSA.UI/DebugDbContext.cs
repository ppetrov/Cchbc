using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.SQLite;
using Windows.Storage;
using Atos.Client.Data;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed class DebugDbContext : IDbContext
	{
		private string DbPath { get; }

		public DebugDbContext()
		{
			this.DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ifsa.sqlite");
		}

		public int Execute(Query query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			var cn = new SQLiteConnection(DbPath, true);
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

		public void Dispose()
		{
			Debug.WriteLine(@"Dispose DbContext");
		}
	}
}