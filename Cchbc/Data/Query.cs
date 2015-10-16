using System;
using System.Linq;

namespace Cchbc.Data
{
	public sealed class Query<T>
	{
		public string Statement { get; }
		public Func<IFieldDataReader, T> Creator { get; }
		public QueryParameter[] Parameters { get; }

		public Query(string statement, Func<IFieldDataReader, T> creator)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (creator == null) throw new ArgumentNullException(nameof(creator));

			this.Statement = statement;
			this.Creator = creator;
			this.Parameters = Enumerable.Empty<QueryParameter>().ToArray();
		}

		public Query(string statement, Func<IFieldDataReader, T> creator, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (creator == null) throw new ArgumentNullException(nameof(creator));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			this.Statement = statement;
			this.Creator = creator;
			this.Parameters = parameters;
		}
	}
}