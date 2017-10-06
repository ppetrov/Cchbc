using System;
using System.Linq;

namespace Atos.Client.Data
{
	public sealed class Query
	{
		public string Statement { get; }
		public QueryParameter[] Parameters { get; }

		public Query(string statement, QueryParameter[] parameters = null)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));

			this.Statement = statement;
			this.Parameters = parameters ?? Enumerable.Empty<QueryParameter>().ToArray();
		}

		public static int GetInt(IFieldDataReader r, int index, int defaultValue = 0)
		{
			return r.IsDbNull(index) ? defaultValue : r.GetInt32(index);
		}

		public static long GetLong(IFieldDataReader r, int index, long defaultValue = 0)
		{
			return r.IsDbNull(index) ? defaultValue : r.GetInt64(index);
		}

		public static string GetString(IFieldDataReader r, int index, string defaultValue = "")
		{
			return r.IsDbNull(index) ? defaultValue : r.GetString(index);
		}
	}

	public sealed class Query<T>
	{
		public string Statement { get; }
		public Func<IFieldDataReader, T> Creator { get; }
		public QueryParameter[] Parameters { get; }

		public Query(string statement, Func<IFieldDataReader, T> creator, QueryParameter[] parameters = null)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (creator == null) throw new ArgumentNullException(nameof(creator));

			this.Statement = statement;
			this.Creator = creator;
			this.Parameters = parameters ?? Enumerable.Empty<QueryParameter>().ToArray();
		}
	}
}