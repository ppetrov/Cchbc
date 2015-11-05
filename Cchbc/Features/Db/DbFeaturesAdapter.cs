using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeaturesAdapter
	{
		private readonly Query<DbFeatureContext> _contextsQuery = new Query<DbFeatureContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbFeatureContextCreator);
		private readonly Query<DbFeatureStep> _stepsQuery = new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator);
		private readonly Query<DbFeature> _featuresQuery = new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureCreator);

		private readonly QueryParameter[] _insertContextSqlParams =
		{
			new QueryParameter(@"NAME", string.Empty),
		};

		private readonly QueryParameter[] _insertStepSqlParams =
		{
			new QueryParameter(@"NAME", string.Empty),
		};

		private readonly QueryParameter[] _insertFeatureSqlParams =
		{
			new QueryParameter(@"@NAME", string.Empty),
			new QueryParameter(@"@CONTEXT", 0L),
		};

		private readonly QueryParameter[] _insertFeatureEntrySqlParams =
		{
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
			new QueryParameter(@"@FEATURE", 0L),
		};

		private readonly QueryParameter[] _insertFeatureStepEntrySqlParams =
		{
			new QueryParameter(@"@ENTRY", 0L),
			new QueryParameter(@"@STEP", 0L),
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
		};

		private readonly Query<long> _selectNewIdQuery = new Query<long>(@"SELECT LAST_INSERT_ROWID()", r => r.GetInt64(0));

		private QueryHelper QueryHelper { get; }

		public DbFeaturesAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task CreateSchemaAsync()
		{
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
	[Details] nvarchar(254) NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)");

			var entryStepsTask = this.QueryHelper.ExecuteAsync(@"
CREATE TABLE [FEATURE_ENTRY_STEPS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NULL, 
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

			// Set parameters values
			_insertContextSqlParams[0].Value = name;

			// Insert the record
			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", _insertContextSqlParams);

			// Get new Id back
			return new DbFeatureContext(await GetNewIdAsync(), name);
		}

		public async Task<DbFeature> InsertFeatureAsync(DbFeatureContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			_insertFeatureSqlParams[0].Value = name;
			_insertFeatureSqlParams[1].Value = context.Id;

			// Insert the record
			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", _insertFeatureSqlParams);

			// Get new Id back
			return new DbFeature(await GetNewIdAsync(), name, context.Id);
		}

		public async Task<DbFeatureEntry> InsertFeatureEntryAsync(DbFeature feature, FeatureEntry featureEntry)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var timeSpent = featureEntry.TimeSpent;
			var details = featureEntry.Details;

			// Set parameters values
			_insertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
			_insertFeatureEntrySqlParams[1].Value = details;
			_insertFeatureEntrySqlParams[2].Value = feature.Id;

			// Insert the record
			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @FEATURE)", _insertFeatureEntrySqlParams);

			// Get new Id back
			return new DbFeatureEntry(await GetNewIdAsync(), feature, details, timeSpent);
		}

		public async Task<DbFeatureStep> InsertStepAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			_insertStepSqlParams[0].Value = name;

			// Insert the record
			await this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", _insertStepSqlParams);

			// Get new Id back
			return new DbFeatureStep(await GetNewIdAsync(), name);
		}

		public Task InsertStepEntryAsync(DbFeatureEntry featureEntry, DbFeatureStep step, FeatureEntryStep entryStep)
		{
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));
			if (step == null) throw new ArgumentNullException(nameof(step));
			if (entryStep == null) throw new ArgumentNullException(nameof(entryStep));

			// Set parameters values
			_insertFeatureStepEntrySqlParams[0].Value = featureEntry.Id;
			_insertFeatureStepEntrySqlParams[1].Value = step.Id;
			_insertFeatureStepEntrySqlParams[2].Value = Convert.ToDecimal(entryStep.TimeSpent.TotalMilliseconds);
			_insertFeatureStepEntrySqlParams[3].Value = entryStep.Details;

			// Insert the record
			return this.QueryHelper.ExecuteAsync(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", _insertFeatureStepEntrySqlParams);
		}

		private static DbFeature DbFeatureCreator(IFieldDataReader r)
		{
			return new DbFeature(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static DbFeatureStep DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStep(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureContext DbFeatureContextCreator(IFieldDataReader r)
		{
			return new DbFeatureContext(r.GetInt64(0), r.GetString(1));
		}

		private async Task<long> GetNewIdAsync()
		{
			var ids = await this.QueryHelper.ExecuteAsync(_selectNewIdQuery);
			return ids[0];
		}
	}
}