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
		public DbFeatureModuleAdapter Adapter { get; set; }

		public Dictionary<string, DbFeatureContext> Contexts { get; } = new Dictionary<string, DbFeatureContext>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		private List<DbFeature> Features { get; } = new List<DbFeature>();

		public DbFeatureModule(DbFeatureModuleAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public async Task LoadAsync()
		{
			var contextTesk = this.LoadContextsAsync();
			var stepsTask = this.LoadStepsAsync();

			await contextTesk;
			await stepsTask;
			await this.LoadFeaturesAsync(this.Contexts);
		}

		public async Task SaveAsync(FeatureEntry entry)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = await this.SaveContextAsync(entry.Context);
			var featureEntry = await this.SaveFeatureAsync(context, entry);
			await this.SaveStepsAsync(featureEntry, entry.Steps);
		}

		private async Task LoadContextsAsync()
		{
			this.Contexts.Clear();
			foreach (var context in await this.Adapter.GetContextsAsync())
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		public async Task LoadStepsAsync()
		{
			this.Steps.Clear();
			foreach (var step in await this.Adapter.GetStepsAsync())
			{
				this.Steps.Add(step.Name, step);
			}
		}

		public async Task LoadFeaturesAsync(Dictionary<string, DbFeatureContext> contexts)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			this.Features.Clear();
			this.Features.AddRange(await this.Adapter.GetAsync(contexts));
		}

		public async Task<DbFeatureEntry> SaveFeatureAsync(DbFeatureContext context, FeatureEntry entry)
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
			return await this.Adapter.InsertEntryAsync(feature, entry.TimeSpent);
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

		private async Task SaveStepsAsync(DbFeatureEntry entry, List<FeatureStep> steps)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (steps == null) throw new ArgumentNullException(nameof(steps));

			foreach (var step in steps)
			{
				var name = step.Name;

				DbFeatureStep current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					current = await this.Adapter.InsertStepAsync(name);
					this.Steps.Add(name, current);
				}
				await this.Adapter.InsertStepAsync(entry, current, step.TimeSpent);
			}
		}

		private async Task<DbFeatureContext> SaveContextAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			DbFeatureContext context;
			if (!Contexts.TryGetValue(name, out context))
			{
				// Insert into database
				context = await this.Adapter.InsertContextAsync(name);

				// Insert the new context into the collection
				this.Contexts.Add(name, context);
			}

			return context;
		}
	}

	public sealed class DbFeatureModuleAdapter
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

		private DbDataAdapter Adapter { get; }

		public DbFeatureModuleAdapter(DbDataAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public Task<List<DbFeatureContext>> GetContextsAsync()
		{
			return this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", this.DbFeatureContextCreator));
		}

		public Task<List<DbFeatureStep>> GetStepsAsync()
		{
			return this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", this.DbFeatureStepCreator));
		}

		public async Task<List<DbFeature>> GetAsync(Dictionary<string, DbFeatureContext> contexts)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			var lookup = new Dictionary<long, DbFeatureContext>();
			foreach (var context in contexts.Values)
			{
				lookup.Add(context.Id, context);
			}

			var rows = await this.Adapter.QueryHelper.ExecuteAsync(new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", this.DbFeatureRowCreator));

			var features = new List<DbFeature>(rows.Count);

			foreach (var row in rows)
			{
				features.Add(ConvertToDbFeature(row, lookup[row.ContextId]));
			}

			return features;
		}

		public async Task<DbFeatureContext> InsertContextAsync(string name)
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

		public async Task<DbFeatureStep> InsertStepAsync(string name)
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

		public async Task InsertStepAsync(DbFeatureEntry entry, DbFeatureStep step, TimeSpan timeSpent)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (step == null) throw new ArgumentNullException(nameof(step));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ENTRY", entry.Id),
				new QueryParameter(@"@STEP", step.Id),
				new QueryParameter(@"@TIMESPENT", Convert.ToDecimal(timeSpent.TotalMilliseconds)),
			};

			await this.Adapter.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT) VALUES (@ENTRY, @STEP, @TIMESPENT)", sqlParams);
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

		private static DbFeature ConvertToDbFeature(DbFeatureRow row, DbFeatureContext context)
		{
			return new DbFeature(row.Id, row.Name, context);
		}

		private DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private DbFeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryRow(r.GetInt64(0), r.GetInt64(1), r.GetDecimal(2));
		}

		private DbFeatureStep DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStep(r.GetInt64(0), r.GetString(1));
		}

		private DbFeatureContext DbFeatureContextCreator(IFieldDataReader r)
		{
			return new DbFeatureContext(r.GetInt64(0), r.GetString(1));
		}
	}
}