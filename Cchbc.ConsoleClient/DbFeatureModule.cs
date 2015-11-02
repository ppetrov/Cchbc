using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.ConsoleClient
{
	public sealed class DbFeatureContext : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

		public DbFeatureContext(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class DbFeature : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public DbFeatureContext Context { get; set; }

		public DbFeature(long id, string name, DbFeatureContext context)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Id = id;
			this.Name = name;
			this.Context = context;
		}
	}

	public sealed class DbFeatureStep : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }

		public DbFeatureStep(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class DbFeatureEntry : IDbObject
	{
		public long Id { get; set; }
		public DbFeature Feature { get; set; }
		public TimeSpan TimeSpent { get; set; }

		public DbFeatureEntry(long id, DbFeature feature, TimeSpan timeSpent)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Id = id;
			this.Feature = feature;
			this.TimeSpent = timeSpent;
		}
	}

	public sealed class DbFeatureStepEntry : IDbObject
	{
		public long Id { get; set; }
		public DbFeatureEntry Entry { get; set; }
		public DbFeatureStep Step { get; set; }
		public double TimeSpent { get; set; }

		public DbFeatureStepEntry(long id, DbFeatureEntry entry, DbFeatureStep step, double timeSpent)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (step == null) throw new ArgumentNullException(nameof(step));

			this.Id = id;
			this.Entry = entry;
			this.Step = step;
			this.TimeSpent = timeSpent;
		}
	}

	public sealed class DbFeatureModule
	{
		public DbFeatureContextManager ContextManager { get; set; }
		public DbFeatureManager FeatureManager { get; set; }
		public DbFeatureStepManager FeatureStepManager { get; set; }

		public DbFeatureModule(DbFeatureContextManager contextManager, DbFeatureManager featureManager, DbFeatureStepManager featureStepManager)
		{
			if (contextManager == null) throw new ArgumentNullException(nameof(contextManager));
			if (featureManager == null) throw new ArgumentNullException(nameof(featureManager));
			if (featureStepManager == null) throw new ArgumentNullException(nameof(featureStepManager));

			this.ContextManager = contextManager;
			this.FeatureManager = featureManager;
			this.FeatureStepManager = featureStepManager;
		}

		public async Task LoadAsync()
		{
			var contextTesk = this.ContextManager.LoadAsync();
			var stepsTask = this.FeatureStepManager.LoadAsync();

			await contextTesk;
			await stepsTask;
			await this.FeatureManager.LoadAsync(this.ContextManager.Contexts);
		}

		public async Task SaveAsync(FeatureEntry entry)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = await this.ContextManager.SaveAsync(entry.Context);
			var featureEntry = await this.FeatureManager.SaveAsync(context, entry);
			await this.FeatureStepManager.SaveAsync(featureEntry, entry.Steps);
		}
	}


	public sealed class DbFeatureContextAdapter
	{
		public DataAdapter Adapter { get; }

		public DbFeatureContextAdapter(DataAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task<Dictionary<long, DbFeatureContext>> GetAsync()
		{
			var contexts = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", this.DbFeatureContextCreator));

			var lookup = new Dictionary<long, DbFeatureContext>(contexts.Count);

			foreach (var context in contexts)
			{
				lookup.Add(context.Id, context);
			}

			return lookup;
		}

		public async Task<DbFeatureContext> InsertAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
			};

			// Insert the record
			await this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", sqlParams);

			// Get the record back
			var contexts = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureContext>(@"SELECT LAST_INSERT_ROWID(), @NAME", this.DbFeatureContextCreator, sqlParams));
			return contexts[0];
		}

		public Task UpdateAsync(DbFeatureContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", context.Id),
				new QueryParameter(@"@NAME", context.Name),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"UPDATE FEATURE_CONTEXTS SET NAME = @NAME WHERE ID = @ID", sqlParams);
		}

		public Task DeleteAsync(DbFeatureContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", context.Id),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"DELETE FROM FEATURE_CONTEXTS WHERE ID = @ID", sqlParams);
		}

		private DbFeatureContext DbFeatureContextCreator(IFieldDataReader r)
		{
			return new DbFeatureContext(r.GetInt64(0), r.GetString(1));
		}
	}




	public sealed class DbFeatureStepAdapter
	{
		public DataAdapter Adapter { get; }

		public DbFeatureStepAdapter(DataAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public Task<List<DbFeatureStep>> GetAsync()
		{
			return this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", this.DbFeatureStepCreator));
		}

		public async Task<DbFeatureStep> InsertAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
			};

			// Insert the record
			await this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", sqlParams);

			// Get the record back
			var steps = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureStep>(@"SELECT LAST_INSERT_ROWID(), @NAME", this.DbFeatureStepCreator));
			return steps[0];
		}

		public Task UpdateAsync(DbFeatureStep step)
		{
			if (step == null) throw new ArgumentNullException(nameof(step));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", step.Id),
				new QueryParameter(@"@NAME", step.Name),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"UPDATE FEATURE_STEPS SET NAME = @NAME WHERE ID = @ID", sqlParams);
		}

		public Task DeleteAsync(DbFeatureStep step)
		{
			if (step == null) throw new ArgumentNullException(nameof(step));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", step.Id),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"DELETE FROM FEATURE_STEPS WHERE ID = @ID", sqlParams);
		}

		private DbFeatureStep DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStep(r.GetInt64(0), r.GetString(1));
		}
	}

	public sealed class DbFeatureAdapter
	{
		private sealed class DbFeatureRow : IDbObject
		{
			public long Id { get; set; }
			public string Name { get; }
			public long ContextId { get; }

			public DbFeatureRow(long id, string name, long contextId)
			{
				this.Id = id;
				this.Name = name;
				this.ContextId = contextId;
			}
		}

		public DataAdapter Adapter { get; }

		public DbFeatureAdapter(DataAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task<List<DbFeature>> GetAsync(Dictionary<long, DbFeatureContext> contexts)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			var rows = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", this.DbFeatureRowCreator));

			var features = new List<DbFeature>(rows.Count);

			foreach (var row in rows)
			{
				features.Add(ConvertToDbFeature(row, contexts[row.ContextId]));
			}

			return features;
		}

		public async Task<DbFeature> InsertAsync(string name, DbFeatureContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
				new QueryParameter(@"@CONTEXT", context.Id),
			};

			await this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", sqlParams);

			// Get the record back
			var rows = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureRow>(@"SELECT LAST_INSERT_ROWID(), @NAME, @CONTEXT", this.DbFeatureRowCreator, sqlParams));
			return ConvertToDbFeature(rows[0], context);
		}

		public Task UpdateAsync(DbFeature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", feature.Id),
				new QueryParameter(@"@NAME", feature.Name),
				new QueryParameter(@"@CONTEXT", feature.Context.Id),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"UPDATE FEATURES SET NAME = @NAME, CONTEXT_ID = @CONTEXT WHERE ID = @ID", sqlParams);
		}

		public Task DeleteAsync(DbFeature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", feature.Id),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"DELETE FROM FEATURES WHERE ID = @ID", sqlParams);
		}

		private DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static DbFeature ConvertToDbFeature(DbFeatureRow row, DbFeatureContext context)
		{
			return new DbFeature(row.Id, row.Name, context);
		}
	}

	public sealed class DbEntryAdapter
	{
		private sealed class DbFeatureEntryRow : IDbObject
		{
			public long Id { get; set; }
			public long Feature { get; }
			public decimal TimeSpent { get; }

			public DbFeatureEntryRow(long id, long feature, decimal timeSpent)
			{
				Id = id;
				Feature = feature;
				TimeSpent = timeSpent;
			}
		}

		public DataAdapter Adapter { get; }

		public DbEntryAdapter(DataAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task<DbFeatureEntry> InsertEntryAsync(DbFeature feature, TimeSpan timeSpent)
		{
			var sqlParams = new[]
			{
				new QueryParameter(@"@FEATURE", feature.Id),
				new QueryParameter(@"@TIMESPENT", Convert.ToDecimal(timeSpent.TotalMilliseconds)),
			};

			await this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRIES(FEATURE_ID, TIMESPENT) VALUES (@FEATURE, @TIMESPENT)", sqlParams);

			// Get the record back
			var rows = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureEntryRow>(@"SELECT LAST_INSERT_ROWID(), @FEATURE, @TIMESPENT", this.DbFeatureEntryRowCreator, sqlParams));
			return new DbFeatureEntry(rows[0].Id, feature, timeSpent);
		}

		public async Task InsertStepAsync(DbFeatureEntry feature, DbFeatureStep timeSpent, TimeSpan timeSpan)
		{
			throw new NotImplementedException();
			//var sqlParams = new[]
			//{
			//	new QueryParameter(@"@FEATURE", feature.Id),
			//	new QueryParameter(@"@TIMESPENT", Convert.ToDecimal(timeSpent.TotalMilliseconds)),
			//};

			//await this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRIES(FEATURE_ID, TIMESPENT) VALUES (@FEATURE, @TIMESPENT)", sqlParams);

			//// Get the record back
			//var rows = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureEntryRow>(@"SELECT LAST_INSERT_ROWID(), @FEATURE, @TIMESPENT", this.DbFeatureEntryRowCreator, sqlParams));
			//return new DbFeatureEntry(rows[0].Id, feature, timeSpent);
		}

		private DbFeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryRow(r.GetInt64(0), r.GetInt64(1), r.GetDecimal(2));
		}


	}


	public sealed class DbFeatureContextManager
	{
		private DbFeatureContextAdapter Adapter { get; }
		public Dictionary<long, DbFeatureContext> Contexts { get; set; } = new Dictionary<long, DbFeatureContext>();

		public DbFeatureContextManager(DbFeatureContextAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task LoadAsync()
		{
			this.Contexts = await this.Adapter.GetAsync();
		}

		public async Task<DbFeatureContext> SaveAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Check if the context exists
			var context = this.FindByName(name);
			if (context == null)
			{
				// Insert into database
				context = await this.Adapter.InsertAsync(name);

				// Insert the new context into the collection
				this.Contexts.Add(context.Id, context);
			}

			return context;
		}

		private DbFeatureContext FindByName(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			foreach (var context in this.Contexts.Values)
			{
				if (context.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return context;
				}
			}

			return null;
		}
	}

	public sealed class DbFeatureManager
	{
		private DbFeatureAdapter Adapter { get; }
		private DbEntryAdapter EntryAdapter { get; }
		private List<DbFeature> Features { get; } = new List<DbFeature>();

		public DbFeatureManager(DbFeatureAdapter adapter, DbEntryAdapter entryAdapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (entryAdapter == null) throw new ArgumentNullException(nameof(entryAdapter));

			this.Adapter = adapter;
			this.EntryAdapter = entryAdapter;
		}

		public async Task LoadAsync(Dictionary<long, DbFeatureContext> contexts)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			this.Features.Clear();
			this.Features.AddRange(await this.Adapter.GetAsync(contexts));
		}

		public async Task<DbFeatureEntry> SaveAsync(DbFeatureContext context, FeatureEntry entry)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var name = entry.Name;

			// Check if the context exists
			var feature = this.FindByContext(context, name);
			if (feature == null)
			{
				// Insert into database
				feature = await this.Adapter.InsertAsync(name, context);

				// Insert the new feature into the collection
				this.Features.Add(feature);
			}

			// Insert into database
			return await this.EntryAdapter.InsertEntryAsync(feature, entry.TimeSpent);
		}

		private DbFeature FindByContext(DbFeatureContext context, string name)
		{
			foreach (var feature in this.Features)
			{
				if (feature.Context == context && feature.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return feature;
				}
			}

			return null;
		}
	}

	public sealed class DbFeatureStepManager
	{
		private DbFeatureStepAdapter Adapter { get; }
		public DbEntryAdapter EntryAdapter { get; }
		public List<DbFeatureStep> Steps { get; } = new List<DbFeatureStep>();

		public DbFeatureStepManager(DbFeatureStepAdapter adapter, DbEntryAdapter entryAdapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (entryAdapter == null) throw new ArgumentNullException(nameof(entryAdapter));

			this.Adapter = adapter;
			this.EntryAdapter = entryAdapter;
		}

		public async Task LoadAsync()
		{
			this.Steps.Clear();
			this.Steps.AddRange(await this.Adapter.GetAsync());
		}

		public async Task SaveAsync(DbFeatureEntry entry, List<FeatureStep> steps)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (steps == null) throw new ArgumentNullException(nameof(steps));

			foreach (var step in steps)
			{
				var current = FindByName(step.Name);
				if (current == null)
				{
					current = await this.Adapter.InsertAsync(step.Name);
					this.Steps.Add(current);
				}

				await this.EntryAdapter.InsertStepAsync(entry, current, step.TimeSpent);
			}
		}

		private DbFeatureStep FindByName(string name)
		{
			foreach (var step in this.Steps)
			{
				if (step.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return step;
				}
			}
			return null;
		}
	}
}