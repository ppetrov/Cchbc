using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.Exceptions.Db
{
	public sealed class DbException : IDbObject
	{
		public long Id { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
	}

	public sealed class DbExceptionContext : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}


	public sealed class DbExceptionManager
	{
		public DbExceptionAdapter Adapter { get; }

		private Dictionary<string, DbExceptionContext> Contexts { get; } = new Dictionary<string, DbExceptionContext>(StringComparer.OrdinalIgnoreCase);

		public DbExceptionManager(DbExceptionAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public Task CreateSchemaAsync()
		{
			return this.Adapter.CreateSchemaAsync();
		}

		public Task LoadAsync()
		{
			return this.LoadContextsAsync();
		}

		public async Task SaveAsync(ExceptionEntry exceptionEntry)
		{
			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

			var context = await this.SaveContextAsync(exceptionEntry.Context);
			await this.SaveExceptionAsync(context, exceptionEntry);
		}

		private async Task LoadContextsAsync()
		{
			// Clear contexts from old values
			this.Contexts.Clear();

			// Fetch & add new values
			foreach (var context in await this.Adapter.GetContextsAsync())
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		private async Task<DbExceptionContext> SaveContextAsync(string name)
		{
			DbExceptionContext context;

			if (!this.Contexts.TryGetValue(name, out context))
			{
				// Insert into database
				context = await this.Adapter.InsertContextAsync(name);

				// Insert the new context into the collection
				this.Contexts.Add(name, context);
			}

			return context;
		}

		private Task SaveExceptionAsync(DbExceptionContext context, ExceptionEntry exceptionEntry)
		{
			return this.Adapter.InsertExceptionEntryAsync(context, exceptionEntry);
		}
	}

	public sealed class DbExceptionAdapter
	{
		private QueryHelper QueryHelper { get; }

		public DbExceptionAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task CreateSchemaAsync()
		{
			throw new NotImplementedException();
		}

		public Task<List<DbExceptionContext>> GetContextsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<DbExceptionContext> InsertContextAsync(string name)
		{
			throw new NotImplementedException();
		}

		public Task InsertExceptionEntryAsync(DbExceptionContext context, ExceptionEntry exceptionEntry)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

			throw new NotImplementedException();
		}
	}
}