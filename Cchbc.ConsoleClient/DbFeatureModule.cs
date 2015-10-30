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
		public double TimeSpent { get; set; }

		public DbFeatureEntry(long id, DbFeature feature, double timeSpent)
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
			var steps = await this.FeatureStepManager.SaveAsync(featureEntry, entry.Steps);

			// TODO : !!!
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

		public Task<List<DbFeatureContext>> GetAsync()
		{
			return this.Adapter.QueryHelper.ExecuteAsync(
				new Query<DbFeatureContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS",
				r => new DbFeatureContext(r.GetInt64(0), r.GetString(1))));
		}

		public async Task<DbFeatureContext> GetAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
			};

			var contexts = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS WHERE NAME = @NAME", r => new DbFeatureContext(r.GetInt64(0), r.GetString(1)), sqlParams));

			if (contexts.Count > 0)
			{
				return contexts[0];
			}
			return null;
		}

		public Task InsertAsync(DbFeatureContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", context.Name),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", sqlParams);
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
			return this.Adapter.QueryHelper.ExecuteAsync(
				new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS",
				r => new DbFeatureStep(r.GetInt64(0), r.GetString(1))));
		}

		public Task InsertAsync(DbFeatureStep step)
		{
			if (step == null) throw new ArgumentNullException(nameof(step));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", step.Name),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", sqlParams);
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
	}

	public sealed class DbFeatureAdapter
	{
		public DataAdapter Adapter { get; }

		public DbFeatureAdapter(DataAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public Task<List<DbFeature>> GetAsync(List<DbFeatureContext> contexts)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			return this.Adapter.QueryHelper.ExecuteAsync(
				new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES",
					r => new DbFeature(r.GetInt64(0), r.GetString(1), contexts.Find(c => c.Id == r.GetInt64(2)))));
		}

		public async Task<DbFeature> GetAsync(DbFeatureContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
			};

			var features = await this.Adapter.QueryHelper.ExecuteAsync(
				new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES WHERE CONTEXT_ID = @CONTEXT AND NAME = @NAME",
					r => new DbFeature(r.GetInt64(0), r.GetString(1), context), sqlParams));

			if (features.Count > 0)
			{
				return features[0];
			}

			return null;
		}

		public Task InsertAsync(DbFeature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", feature.Name),
				new QueryParameter(@"@CONTEXT", feature.Context.Id),
			};

			return this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", sqlParams);
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
	}

	public sealed class DbFeatureContextManager
	{
		private DbFeatureContextAdapter Adapter { get; }
		public List<DbFeatureContext> Contexts { get; } = new List<DbFeatureContext>();

		public DbFeatureContextManager(DbFeatureContextAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task LoadAsync()
		{
			this.Contexts.Clear();
			this.Contexts.AddRange(await this.Adapter.GetAsync());
		}

		public async Task<DbFeatureContext> SaveAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Check if the context exists
			var context = this.FindByName(name);
			if (context == null)
			{
				// Insert in the database
				await this.Adapter.InsertAsync(new DbFeatureContext(-1, name));

				// Get the new context back from the database
				context = await this.Adapter.GetAsync(name);

				// Insert the new context into the collection
				this.Contexts.Add(context);
			}

			return context;
		}

		private DbFeatureContext FindByName(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			foreach (var context in this.Contexts)
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
		private List<DbFeature> Features { get; } = new List<DbFeature>();

		public DbFeatureManager(DbFeatureAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task LoadAsync(List<DbFeatureContext> contexts)
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
				// Insert in the database
				await this.Adapter.InsertAsync(new DbFeature(-1, name, context));

				// Get the new feature back from the database
				feature = await this.Adapter.GetAsync(context, name);

				// Insert the new feature into the collection
				this.Features.Add(feature);
			}

			var dbFeatureEntry = new DbFeatureEntry(-1, feature, entry.TimeSpent.TotalMilliseconds);

			// TODO : !!! Insert into database !!!!
			

			return dbFeatureEntry;
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
		public List<DbFeatureStep> Steps { get; } = new List<DbFeatureStep>();

		public DbFeatureStepManager(DbFeatureStepAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task LoadAsync()
		{
			this.Steps.Clear();
			this.Steps.AddRange(await this.Adapter.GetAsync());
		}

		// TODO : !!!
		public async Task<List<DbFeatureStepEntry>> SaveAsync(DbFeatureEntry entry, List<FeatureStep> steps)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (steps == null) throw new ArgumentNullException(nameof(steps));

			var stepEntries = new List<DbFeatureStepEntry>(steps.Count);

			foreach (var step in steps)
			{
				var current = default(DbFeatureStep);

				var exists = true;
				if (!exists)
				{
					// TODO : Insert
				}

				stepEntries.Add(new DbFeatureStepEntry(-1, null, current, step.TimeSpent.TotalMilliseconds));
			}

			return stepEntries;
		}

		//public async Task<DbFeatureContext> SaveAsync(string name)
		//{
		//	if (name == null) throw new ArgumentNullException(nameof(name));

		//	// Check if the context exists
		//	var context = this.FindByName(name);
		//	if (context == null)
		//	{
		//		// Insert in the database
		//		await this.Adapter.InsertAsync(new DbFeatureContext(-1, name));

		//		// Get the new context back from the database
		//		context = await this.Adapter.GetAsync(name);

		//		// Insert the new context into the collection
		//		this.Contexts.Add(context);
		//	}

		//	return context;
		//}

		//private DbFeatureContext FindByName(string name)
		//{
		//	if (name == null) throw new ArgumentNullException(nameof(name));

		//	foreach (var context in this.Contexts)
		//	{
		//		if (context.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
		//		{
		//			return context;
		//		}
		//	}

		//	return null;
		//}
	}
}