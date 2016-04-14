using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Db.Adapters
{
	public static class DbFeatureAdapter
	{
		private static readonly QueryParameter[] InsertContextSqlParams =
		{
			new QueryParameter(@"NAME", string.Empty),
		};

		private static readonly QueryParameter[] InsertStepSqlParams =
		{
			new QueryParameter(@"NAME", string.Empty),
		};

		private static readonly QueryParameter[] InsertFeatureSqlParams =
		{
			new QueryParameter(@"@NAME", string.Empty),
			new QueryParameter(@"@CONTEXT", 0L),
		};

		private static readonly QueryParameter[] InsertFeatureEntrySqlParams =
{
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
			new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
			new QueryParameter(@"@FEATURE", 0L),
		};

		private static readonly QueryParameter[] InsertExcetionEntrySqlParams =
		{
			new QueryParameter(@"@MESSAGE", string.Empty),
			new QueryParameter(@"@STACKTRACE", string.Empty),
			new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
			new QueryParameter(@"@FEATURE", 0L),
		};

		private static readonly QueryParameter[] InsertFeatureStepEntrySqlParams =
		{
			new QueryParameter(@"@ENTRY", 0L),
			new QueryParameter(@"@STEP", 0L),
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
		};

		public static void CreateClientSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			CreateCommonSchema(context);

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NOT NULL, 
	[CreatedAt] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_EXCEPTIONS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Message] nvarchar(254) NOT NULL, 
	[StackTrace] nvarchar(254) NOT NULL, 
	[CreatedAt] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
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
)"));
		}

		public static void CreateServerSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			CreateCommonSchema(context);


			context.Execute(new Query(@"
CREATE TABLE[FEATURE_USERS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NULL, 
	[CreatedAt] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	[User_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([User_Id])
		REFERENCES [FEATURE_USERS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_EXCEPTIONS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Message] nvarchar(254) NOT NULL, 
	[StackTrace] nvarchar(254) NOT NULL, 
	[CreatedAt] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
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
)"));
		}

		public static void DropClientSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRY_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTIONS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
		}

		public static void DropServerSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRY_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTIONS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
		}










		public static List<DbContextRow> GetContexts(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(new Query<DbContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextCreator));
		}

		public static List<DbFeatureStepRow> GetSteps(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(new Query<DbFeatureStepRow>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator));
		}

		public static List<DbFeatureRow> GetFeatures(ITransactionContext context, Dictionary<string, DbContextRow> contexts)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			var features = context.Execute(new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator));
			if (features.Count == 0) return features;

			var contextsById = new Dictionary<long, long>();
			foreach (var dbContext in contexts.Values)
			{
				contextsById.Add(dbContext.Id, dbContext.Id);
			}

			var mappedFeatures = new List<DbFeatureRow>(features.Count);
			foreach (var feature in features)
			{
				mappedFeatures.Add(new DbFeatureRow(feature.Id, feature.Name, contextsById[feature.ContextId]));
			}

			return mappedFeatures;
		}

		public static long InsertContext(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertContextSqlParams[0].Value = name;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", InsertContextSqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		public static long InsertStep(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertStepSqlParams[0].Value = name;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", InsertStepSqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		public static long InsertFeature(ITransactionContext context, string name, long contextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureSqlParams[0].Value = name;
			InsertFeatureSqlParams[1].Value = contextId;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", InsertFeatureSqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static void CreateCommonSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"
CREATE TABLE[FEATURE_CONTEXTS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE[FEATURE_STEPS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Name] nvarchar(254) NOT NULL, 
	[Context_Id] integer NOT NULL, 
	FOREIGN KEY ([Context_Id])
		REFERENCES [FEATURE_CONTEXTS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
		}

		public static DbFeatureEntryRow InsertFeatureEntry(ITransactionContext context, long featureId, Feature feature, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (details == null) throw new ArgumentNullException(nameof(details));

			var timeSpent = Convert.ToDecimal(feature.TimeSpent.TotalMilliseconds);
			var dateTime = DateTime.Now;

			// Set parameters values
			InsertFeatureEntrySqlParams[0].Value = timeSpent;
			InsertFeatureEntrySqlParams[1].Value = details;
			InsertFeatureEntrySqlParams[2].Value = dateTime;
			InsertFeatureEntrySqlParams[3].Value = featureId;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE)", InsertFeatureEntrySqlParams));

			// Get new Id back
			return new DbFeatureEntryRow(context.GetNewId(), timeSpent, details, dateTime, featureId);
		}

		public static void InsertExceptionEntry(ITransactionContext context, long featureId, FeatureException featureException)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (featureException == null) throw new ArgumentNullException(nameof(featureException));

			// Set parameters values
			InsertExcetionEntrySqlParams[0].Value = featureException.Exception.Message;
			InsertExcetionEntrySqlParams[1].Value = featureException.Exception.StackTrace;
			InsertExcetionEntrySqlParams[2].Value = DateTime.Now;
			InsertExcetionEntrySqlParams[3].Value = featureId;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_EXCEPTIONS(MESSAGE, STACKTRACE, CREATEDAT, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATEDAT, @FEATURE)", InsertExcetionEntrySqlParams));
		}

		public static void InsertStepEntry(ITransactionContext context, DbFeatureEntryRow featureEntryRow, DbFeatureStepRow stepRow, FeatureStep entryStep)
		{
			if (featureEntryRow == null) throw new ArgumentNullException(nameof(featureEntryRow));
			if (stepRow == null) throw new ArgumentNullException(nameof(stepRow));
			if (entryStep == null) throw new ArgumentNullException(nameof(entryStep));

			// Set parameters values
			InsertFeatureStepEntrySqlParams[0].Value = featureEntryRow.Id;
			InsertFeatureStepEntrySqlParams[1].Value = stepRow.Id;
			InsertFeatureStepEntrySqlParams[2].Value = Convert.ToDecimal(entryStep.TimeSpent.TotalMilliseconds);
			InsertFeatureStepEntrySqlParams[3].Value = entryStep.Details;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams));
		}

		private static DbContextRow DbContextCreator(IFieldDataReader r)
		{
			return new DbContextRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureStepRow DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStepRow(r.GetInt64(0), r.GetString(1));
		}
	}
}