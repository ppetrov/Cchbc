using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cchbc.ConsoleClient;
using Cchbc.Data;

namespace ConsoleClient
{
	public sealed class DbContext : IDbContext
	{
		private readonly SQLiteConnection _cn;

		private SQLiteTransaction _tr;

		public DbContext(string cnString)
		{
			if (cnString == null) throw new ArgumentNullException(nameof(cnString));

			// Create connection
			_cn = new SQLiteConnection(cnString);

			// Open connection
			_cn.Open();

			// Begin transaction
			_tr = _cn.BeginTransaction();
		}

		public int Execute(Query query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			using (var cmd = _cn.CreateCommand())
			{
				DisplayQuery(query);

				cmd.Transaction = _tr;
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;

				Debug.WriteLine(query.Statement);

				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				return cmd.ExecuteNonQuery();
			}
		}

		public List<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			var items = new List<T>();

			using (var cmd = _cn.CreateCommand())
			{
				DisplayQuery(query);

				cmd.Transaction = _tr;
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;

				Debug.WriteLine(query.Statement);

				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				using (var r = cmd.ExecuteReader())
				{
					var dr = new SqlLiteDelegateDataReader(r);
					while (dr.Read())
					{
						items.Add(query.Creator(dr));
					}
				}
			}

			return items;
		}

		public void Fill<T>(Dictionary<long, T> items, Func<T, long> selector, Query<T> query)
		{
			using (var cmd = _cn.CreateCommand())
			{
				DisplayQuery(query);

				cmd.Transaction = _tr;
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;

				Debug.WriteLine(query.Statement);

				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				using (var r = cmd.ExecuteReader())
				{
					var dr = new SqlLiteDelegateDataReader(r);
					while (dr.Read())
					{
						var value = query.Creator(dr);
						items.Add(selector(value), value);
					}
				}
			}
		}

		public void Fill<TK, TV>(Dictionary<TK, TV> items, Action<IFieldDataReader, Dictionary<TK, TV>> filler, Query query)
		{
			using (var cmd = _cn.CreateCommand())
			{
				DisplayQuery(query);

				cmd.Transaction = _tr;
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;

				Debug.WriteLine(query.Statement);

				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				using (var r = cmd.ExecuteReader())
				{
					var dr = new SqlLiteDelegateDataReader(r);
					while (dr.Read())
					{
						filler(dr, items);
					}
				}
			}
		}

		public void Complete()
		{
			_tr?.Commit();
			_tr?.Dispose();
			_tr = null;
		}

		public void Dispose()
		{
			try
			{
				_tr?.Rollback();
				_tr?.Dispose();
			}
			finally
			{
				_cn.Dispose();
			}
		}

		private void DisplayQuery(Query query)
		{
			DisplayQuery(query.Statement);
		}

		private void DisplayQuery<T>(Query<T> query)
		{
			DisplayQuery(query.Statement);
		}

		private static void DisplayQuery(string statement)
		{
			if (false)
			{
				Console.WriteLine(Regex.Replace(statement.Replace('\t', ' ').Replace(Environment.NewLine, @" "), @" +", @" ").Trim());
			}
		}
	}
}