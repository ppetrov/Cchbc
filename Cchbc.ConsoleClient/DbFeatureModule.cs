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
		public string Name { get; }
		public long ContextId { get; }

		public DbFeature(long id, string name, long contextId)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}

	public sealed class DbFeatureStep : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

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
		public DbFeature Feature { get; }
		public string Details { get; }
		public TimeSpan TimeSpent { get; }

		public DbFeatureEntry(long id, DbFeature feature, string details, TimeSpan timeSpent)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Id = id;
			this.Feature = feature;
			this.Details = details;
			this.TimeSpent = timeSpent;
		}
	}

	public sealed class DbFeatureStepEntry : IDbObject
	{
		public long Id { get; set; }
		public DbFeatureEntry Entry { get; }
		public DbFeatureStep Step { get; }
		public string Details { get; }
		public double TimeSpent { get; }

		public DbFeatureStepEntry(long id, DbFeatureEntry entry, DbFeatureStep step, string details, double timeSpent)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (step == null) throw new ArgumentNullException(nameof(step));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Id = id;
			this.Entry = entry;
			this.Step = step;
			this.Details = details;
			this.TimeSpent = timeSpent;
		}
	}

	public sealed class DbFeatureModule
	{
		private DbFeatureModuleAdapter Adapter { get; }

		private Dictionary<string, DbFeatureContext> Contexts { get; } = new Dictionary<string, DbFeatureContext>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<long, Dictionary<string, DbFeature>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeature>>();

		public DbFeatureModule(DbFeatureModuleAdapter adapter)
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
			var contextTesk = this.LoadContextsAsync();
			var stepsTask = this.LoadStepsAsync();
			var featuresTask = this.LoadFeaturesAsync();

			return Task.WhenAll(contextTesk, stepsTask, featuresTask);
		}

		// TODO : !! FeatureEntry
		//public string Context { get; }
		//public string Name { get; }
		//public string Details { get; }
		//public TimeSpan TimeSpent => this.Stopwatch.Elapsed;

		// TODO : !!! FeatureStepEntry
		// TODO : MANY !!! or none
		//public string Name { get; }
		//public string Details { get; set; }
		//public TimeSpan TimeSpent { get; set; }

		public async Task SaveAsync(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var context = await this.SaveContextAsync(feature.Context);
			var featureEntry = await this.SaveFeatureAsync(context, feature);
			await this.SaveStepsAsync(featureEntry, feature.Steps);
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

		private async Task LoadStepsAsync()
		{
			// Clear steps from old values
			this.Steps.Clear();

			// Fetch & add new values
			foreach (var step in await this.Adapter.GetStepsAsync())
			{
				this.Steps.Add(step.Name, step);
			}
		}

		private async Task LoadFeaturesAsync()
		{
			// Clear steps from old values
			this.Features.Clear();

			// Fetch & add new values
			foreach (var feature in await this.Adapter.GetFeaturesAsync())
			{
				var contextId = feature.ContextId;

				// Find features by context
				Dictionary<string, DbFeature> byContext;
				if (!this.Features.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature);
			}
		}

		public async Task<DbFeatureEntry> SaveFeatureAsync(DbFeatureContext context, Feature feature)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var name = feature.Name;

			// Check if the context exists
			var dbFeature = this.FindByContext(context, name);
			if (dbFeature == null)
			{
				// Insert into database
				dbFeature = await this.Adapter.InsertAsync(name, context);

				// Insert the new feature into the collection
				var byContext = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);

				byContext.Add(dbFeature.Name, dbFeature);

				this.Features.Add(context.Id, byContext);
			}

			// Insert into database
			return await this.Adapter.InsertEntryAsync(dbFeature, feature.TimeSpent);
		}

		private DbFeature FindByContext(DbFeatureContext context, string name)
		{
			DbFeature feature = null;

			Dictionary<string, DbFeature> features;
			if (this.Features.TryGetValue(context.Id, out features))
			{
				features.TryGetValue(name, out feature);
			}

			return feature;
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
			if (!this.Contexts.TryGetValue(name, out context))
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
		private readonly Query<DbFeatureContext> _contextsQuery = new Query<DbFeatureContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbFeatureContextCreator);
		private readonly Query<DbFeatureStep> _stepsQuery = new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator);
		private readonly Query<DbFeature> _featuresQuery = new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureCreator);

		private QueryHelper QueryHelper { get; }

		public DbFeatureModuleAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task CreateSchemaAsync()
		{
			// TODO : Add details to Feature entry & step entry !!!
			var contextsTask = this.QueryHelper.ExecuteAsync(@"
CREATE TABLE[FEATURE_CONTEXTS] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Name] nvarchar(254) NOT NULL
)");

			var stepsTask = this.QueryHelper.ExecuteAsync(@"
CREATE TABLE[FEATURE_STEPS] (
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Name] nvarchar(254) NOT NULL
)");

			var featuresTask = this.QueryHelper.ExecuteAsync(@"
CREATE TABLE [FEATURES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Name] nvarchar(254) NOT NULL, 
	[Context_Id] integer NOT NULL, 
	FOREIGN KEY ([Context_Id])
		REFERENCES [FEATURE_CONTEXTS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)");

			var entriesTask = this.QueryHelper.ExecuteAsync(@"
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)");

			var entryStepsTask = this.QueryHelper.ExecuteAsync(@"
CREATE TABLE [FEATURE_ENTRY_STEPS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
    [Feature_Entry_Id] integer NOT NULL, 
	[Feature_Step_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Entry_Id])
		REFERENCES [FEATURE_ENTRIES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Feature_Step_Id])
		REFERENCES [FEATURE_STEPS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE		
)");
			return Task.WhenAll(contextsTask, stepsTask, featuresTask, entriesTask, entryStepsTask);
		}

		public Task<List<DbFeatureContext>> GetContextsAsync()
		{
			return this.QueryHelper.ExecuteAsync(_contextsQuery);
		}

		public Task<List<DbFeatureStep>> GetStepsAsync()
		{
			return this.QueryHelper.ExecuteAsync(_stepsQuery);
		}

		public Task<List<DbFeature>> GetFeaturesAsync()
		{
			return this.QueryHelper.ExecuteAsync(_featuresQuery);
		}

		public async Task<DbFeatureContext> InsertContextAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
			};

			// Insert the record
			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", sqlParams);

			// Get new Id back
			var ids = await this.QueryHelper.ExecuteAsync(new Query<long>(@"SELECT LAST_INSERT_ROWID()", IdCreator, sqlParams));
			return new DbFeatureContext(ids[0], name);
		}

		public async Task<DbFeatureStep> InsertStepAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
			};

			// Insert the record
			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", sqlParams);

			// Get new Id back
			var ids = await this.QueryHelper.ExecuteAsync(new Query<long>(@"SELECT LAST_INSERT_ROWID()", IdCreator));
			return new DbFeatureStep(ids[0], name);
		}

		public async Task<DbFeatureEntry> InsertEntryAsync(DbFeature feature, TimeSpan timeSpent)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var sqlParams = new[]
			{
				new QueryParameter(@"@FEATURE", feature.Id),
				new QueryParameter(@"@TIMESPENT", Convert.ToDecimal(timeSpent.TotalMilliseconds)),
			};

			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRIES(FEATURE_ID, TIMESPENT) VALUES (@FEATURE, @TIMESPENT)", sqlParams);

			// Get the record back
			var ids = await this.QueryHelper.ExecuteAsync(new Query<long>(@"SELECT LAST_INSERT_ROWID()", IdCreator));
			return new DbFeatureEntry(ids[0], feature, null, timeSpent);
		}

		public Task InsertStepAsync(DbFeatureEntry entry, DbFeatureStep step, TimeSpan timeSpent)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (step == null) throw new ArgumentNullException(nameof(step));

			var sqlParams = new[]
			{
				new QueryParameter(@"@ENTRY", entry.Id),
				new QueryParameter(@"@STEP", step.Id),
				new QueryParameter(@"@TIMESPENT", Convert.ToDecimal(timeSpent.TotalMilliseconds)),
			};

			return this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT) VALUES (@ENTRY, @STEP, @TIMESPENT)", sqlParams);
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

			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", sqlParams);

			// Get the record back
			var ids = await this.QueryHelper.ExecuteAsync(new Query<long>(@"SELECT LAST_INSERT_ROWID()", IdCreator, sqlParams));
			return new DbFeature(ids[0], name, context.Id);
		}

		private static DbFeature DbFeatureCreator(IFieldDataReader r)
		{
			return new DbFeature(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static long IdCreator(IFieldDataReader r)
		{
			return r.GetInt64(0);
		}

		private static DbFeatureStep DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStep(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureContext DbFeatureContextCreator(IFieldDataReader r)
		{
			return new DbFeatureContext(r.GetInt64(0), r.GetString(1));
		}
	}
}