using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Db.Adapters
{
	public static class DbFeatureAdapter
	{
		private static readonly QueryParameter[] NameSqlParams =
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

		private static readonly QueryParameter[] InsertServerFeatureEntrySqlParams =
{
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
			new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
			new QueryParameter(@"@FEATURE", 0L),
			new QueryParameter(@"@USER", 0L),
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

		private static readonly Query InserUserQuery = new Query(@"INSERT INTO FEATURE_USERS(NAME) VALUES (@NAME)", NameSqlParams);
		private static readonly Query InsertContextQuery = new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", NameSqlParams);
		private static readonly Query InsertStepQuery = new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", NameSqlParams);
		private static readonly Query InsertFeatureQuery = new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", InsertFeatureSqlParams);
		private static readonly Query InsertClientFeatureEntryQuery = new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE)", InsertFeatureEntrySqlParams);
		private static readonly Query InsertServerFeatureEntryQuery = new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID, USER_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE, @USER)", InsertServerFeatureEntrySqlParams);
		private static readonly Query InsertExceptionQuery = new Query(@"INSERT INTO FEATURE_EXCEPTIONS(MESSAGE, STACKTRACE, CREATEDAT, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATEDAT, @FEATURE)", InsertExcetionEntrySqlParams);
		private static readonly Query InsertStepEntryQuery = new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams);

		private static readonly Query<DbContextRow> GetContextsQuery = new Query<DbContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextCreator);
		private static readonly Query<DbFeatureStepRow> GetStepsQuery = new Query<DbFeatureStepRow>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator);
		private static readonly Query<DbFeatureRow> GetFeaturesQuery = new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);
		private static readonly Query<DbFeatureEntryRow> GetFeatureEntriesQuery = new Query<DbFeatureEntryRow>(@"SELECT ID, TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
		private static readonly Query<DbFeatureEntryStepRow> GetFeatureEntryStepsQuery = new Query<DbFeatureEntryStepRow>(@"SELECT TIMESPENT, DETAILS, FEATURE_ENTRY_ID, FEATURE_STEP_ID FROM FEATURE_ENTRY_STEPS", EntryStepRowCreator);
		private static readonly Query<long> GetUserQuery = new Query<long>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", r => r.GetInt64(0), NameSqlParams);

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

		public static void DropClientSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DropSchema(context);
		}

		public static void DropServerSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DropSchema(context);

			context.Execute(new Query(@"DROP TABLE FEATURE_USERS"));
		}

		private static void DropSchema(ITransactionContext context)
		{
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRY_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTIONS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
		}

		




		public static long GetOrCreateUser(ITransactionContext context, string userName)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			// Set parameters values
			NameSqlParams[0].Value = userName;

			// select the record
			var ids = context.Execute(GetUserQuery);
			if (ids.Count > 0)
			{
				return ids[0];
			}

			return ExecureInsert(context, InserUserQuery);
		}

		public static List<DbContextRow> GetContexts(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(GetContextsQuery);
		}

		public static List<DbFeatureStepRow> GetSteps(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(GetStepsQuery);
		}

		public static List<DbFeatureRow> GetFeatures(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(GetFeaturesQuery);
		}

		public static List<DbFeatureEntryRow> GetFeatureEntryRows(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(GetFeatureEntriesQuery);
		}

		public static List<DbFeatureEntryStepRow> GetEntryStepRows(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(GetFeatureEntryStepsQuery);
		}

		public static long InsertContext(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			NameSqlParams[0].Value = name;

			return ExecureInsert(context, InsertContextQuery);
		}

		public static long InsertStep(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			NameSqlParams[0].Value = name;

			return ExecureInsert(context, InsertStepQuery);
		}

		public static long InsertFeature(ITransactionContext context, string name, long contextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureSqlParams[0].Value = name;
			InsertFeatureSqlParams[1].Value = contextId;

			// Insert the record
			return ExecureInsert(context, InsertFeatureQuery);
		}

		public static long InsertFeatureEntry(ITransactionContext context, long featureId, string details, Feature feature)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(feature.TimeSpent.TotalMilliseconds);
			InsertFeatureEntrySqlParams[1].Value = details;
			InsertFeatureEntrySqlParams[2].Value = DateTime.Now;
			InsertFeatureEntrySqlParams[3].Value = featureId;

			return ExecureInsert(context, InsertClientFeatureEntryQuery);
		}

		public static long InsertFeatureEntry(ITransactionContext context, long featureId, string details, decimal timeSpent, DateTime createdAt, long userId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertServerFeatureEntrySqlParams[0].Value = timeSpent;
			InsertServerFeatureEntrySqlParams[1].Value = details;
			InsertServerFeatureEntrySqlParams[2].Value = createdAt;
			InsertServerFeatureEntrySqlParams[3].Value = featureId;
			InsertServerFeatureEntrySqlParams[4].Value = userId;

			return ExecureInsert(context, InsertServerFeatureEntryQuery);
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
			context.Execute(InsertExceptionQuery);
		}

		public static void InsertStepEntry(ITransactionContext context, long featureEntryId, long stepId, decimal timeSpent, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Set parameters values
			InsertFeatureStepEntrySqlParams[0].Value = featureEntryId;
			InsertFeatureStepEntrySqlParams[1].Value = stepId;
			InsertFeatureStepEntrySqlParams[2].Value = timeSpent;
			InsertFeatureStepEntrySqlParams[3].Value = details;

			// Insert the record
			context.Execute(InsertStepEntryQuery);
		}

		private static DbContextRow DbContextCreator(IFieldDataReader r)
		{
			return new DbContextRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureStepRow DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStepRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static DbFeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryRow(r.GetInt64(0), r.GetDecimal(1), r.GetString(2), r.GetDateTime(3), r.GetInt64(4));
		}

		private static DbFeatureEntryStepRow EntryStepRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryStepRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2), r.GetInt64(3));
		}

		private static long ExecureInsert(ITransactionContext context, Query query)
		{
			// Insert the record
			context.Execute(query);

			// Get new Id back
			return context.GetNewId();
		}
	}
}