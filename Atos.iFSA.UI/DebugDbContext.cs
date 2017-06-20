using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.SQLite;
using Windows.Storage;
using Atos.Client.Data;

namespace Atos.iFSA.UI
{
	public sealed class DebugDbContext : IDbContext
	{
		private SQLiteConnection _cn;

		private string DbPath { get; }

		public DebugDbContext()
		{
			this.DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ifsa.sqlite");
		}

		public int Execute(Query query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			this.OpenConnection();

			using (var cmd = _cn.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query.Statement;

				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				return cmd.ExecuteNonQuery();
			}
		}

		public IEnumerable<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			this.OpenConnection();

			using (var cmd = _cn.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = query.Statement;

				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				using (var r = cmd.ExecuteReader())
				{ 
					while (r.Read())
					{
						
					}
				}
			}

			yield break;
		}

		public void Complete()
		{
			Debug.WriteLine(@"Commit SQL transaction");
		}

		public void Dispose()
		{
			Debug.WriteLine(@"Dispose DbContext");
		}

		private void OpenConnection()
		{
			if (_cn != null)
			{
				return;
			}
			_cn = new SQLiteConnection(DbPath, true);

			_cn.Open();
		}
	}
}