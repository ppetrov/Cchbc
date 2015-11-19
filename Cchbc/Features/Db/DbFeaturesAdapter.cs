using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeaturesAdapter
	{
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

		private readonly QueryParameter[] _insertExcetionEntrySqlParams =
		{
			new QueryParameter(@"@MESSAGE", string.Empty),
			new QueryParameter(@"@STACKTRACE", string.Empty),
			new QueryParameter(@"@FEATURE", 0L),
		};

		private readonly QueryParameter[] _insertFeatureStepEntrySqlParams =
		{
			new QueryParameter(@"@ENTRY", 0L),
			new QueryParameter(@"@STEP", 0L),
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
		};

		private QueryHelper QueryHelper { get; }

		public DbFeaturesAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public void CreateSchema()
		{
			this.QueryHelper.Execute(@"
CREATE TABLE[FEATURE_CONTEXTS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)");

			this.QueryHelper.Execute(@"
CREATE TABLE[FEATURE_STEPS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)");

			this.QueryHelper.Execute(@"
CREATE TABLE [FEATURES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Name] nvarchar(254) NOT NULL, 
	[Context_Id] integer NOT NULL, 
	FOREIGN KEY ([Context_Id])
		REFERENCES [FEATURE_CONTEXTS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)");

			this.QueryHelper.Execute(@"
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)");

			this.QueryHelper.Execute(@"
CREATE TABLE [EXCEPTION_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Message] nvarchar(254) NOT NULL, 
	[StackTrace] nvarchar(254) NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)");

			this.QueryHelper.Execute(@"
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
		}

		public List<DbContext> GetContexts()
		{
			return this.QueryHelper.Execute(new Query<DbContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbFeatureContextCreator));
		}

		public List<DbFeatureStep> GetSteps()
		{
			return this.QueryHelper.Execute(new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator));
		}

		public List<DbFeature> GetFeatures()
		{
			return this.QueryHelper.Execute(new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureCreator));
		}

		public DbContext InsertContext(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			_insertContextSqlParams[0].Value = name;

			// Insert the record
			this.QueryHelper.Execute(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", _insertContextSqlParams);

			// Get new Id back
			return new DbContext(this.QueryHelper.GetNewId(), name);
		}

		public DbFeature InsertFeature(DbContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			_insertFeatureSqlParams[0].Value = name;
			_insertFeatureSqlParams[1].Value = context.Id;

			// Insert the record
			this.QueryHelper.Execute(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", _insertFeatureSqlParams);

			// Get new Id back
			return new DbFeature(this.QueryHelper.GetNewId(), name, context.Id);
		}

		public DbFeatureEntry InsertFeatureEntry(DbFeature feature, FeatureEntry featureEntry)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));

			var timeSpent = featureEntry.TimeSpent;
			var details = featureEntry.Details;

			// Set parameters values
			_insertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
			_insertFeatureEntrySqlParams[1].Value = details;
			_insertFeatureEntrySqlParams[2].Value = feature.Id;

			// Insert the record
			this.QueryHelper.Execute(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @FEATURE)", _insertFeatureEntrySqlParams);

			// Get new Id back
			return new DbFeatureEntry(this.QueryHelper.GetNewId(), feature, details, timeSpent);
		}

		public DbFeatureStep InsertStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			_insertStepSqlParams[0].Value = name;

			// Insert the record
			this.QueryHelper.Execute(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", _insertStepSqlParams);

			// Get new Id back
			return new DbFeatureStep(this.QueryHelper.GetNewId(), name);
		}

		public void InsertExceptionEntry(DbFeature feature, ExceptionEntry exceptionEntry)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

			// Set parameters values
			_insertExcetionEntrySqlParams[0].Value = exceptionEntry.Exception.Message;
			_insertExcetionEntrySqlParams[1].Value = exceptionEntry.Exception.StackTrace;
			_insertExcetionEntrySqlParams[2].Value = feature.Id;

			// Insert the record
			this.QueryHelper.Execute(@"INSERT INTO EXCEPTION_ENTRIES(MESSAGE, STACKTRACE, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @FEATURE)", _insertExcetionEntrySqlParams);
		}

		public void InsertStepEntry(DbFeatureEntry featureEntry, DbFeatureStep step, FeatureEntryStep entryStep)
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
			this.QueryHelper.Execute(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", _insertFeatureStepEntrySqlParams);
		}

		private static DbFeature DbFeatureCreator(IFieldDataReader r)
		{
			return new DbFeature(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static DbFeatureStep DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStep(r.GetInt64(0), r.GetString(1));
		}

		private static DbContext DbFeatureContextCreator(IFieldDataReader r)
		{
			return new DbContext(r.GetInt64(0), r.GetString(1));
		}
	}
}