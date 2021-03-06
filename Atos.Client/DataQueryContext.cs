﻿using System;
using System.Collections.Generic;
using Atos.Client.Data;

namespace Atos.Client
{
	public sealed class DataQueryContext : IDisposable
	{
		public MainContext MainContext { get; }
		public IDbContext DbContext { get; }

		public DataQueryContext(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
			this.DbContext = mainContext.GetService<IDbContext>();
		}

		public int Execute(Query query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.DbContext.Execute(query);
		}

		public IEnumerable<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.DbContext.Execute(query);
		}

		public void Complete()
		{
			this.DbContext.Complete();
		}

		public void Dispose()
		{
			this.DbContext.Dispose();
		}
	}
}