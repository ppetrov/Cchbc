using System;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public static class DbFeatureServerAdapter
	{
		private static readonly QueryParameter[] InsertFeatureEntrySqlParams =
		{
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
			new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
			new QueryParameter(@"@FEATURE", 0L),
			new QueryParameter(@"@USER", 0L),
		};

		//private static readonly QueryParameter[] InsertExcetionEntrySqlParams =
		//{
		//	new QueryParameter(@"@MESSAGE", string.Empty),
		//	new QueryParameter(@"@STACKTRACE", string.Empty),
		//	new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
		//	new QueryParameter(@"@FEATURE", 0L),
		//};

		private static readonly QueryParameter[] InsertFeatureStepEntrySqlParams =
		{
			new QueryParameter(@"@ENTRY", 0L),
			new QueryParameter(@"@STEP", 0L),
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
		};

		private static readonly QueryParameter[] InsertUserSqlParams =
		{
			new QueryParameter(@"NAME", string.Empty),
		};

		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureAdapter.CreateSchema(context);

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

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRY_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTIONS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_USERS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
		}

		public static DbFeatureEntry InsertFeatureEntry(ITransactionContext context, DbFeature feature, FeatureEntry featureEntry, DbFeatureUser user)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));
			if (user == null) throw new ArgumentNullException(nameof(user));

			var timeSpent = featureEntry.TimeSpent;
			var details = featureEntry.Details;

			// Set parameters values
			InsertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
			InsertFeatureEntrySqlParams[1].Value = details;
			InsertFeatureEntrySqlParams[2].Value = DateTime.Now;
			InsertFeatureEntrySqlParams[3].Value = feature.Id;
			InsertFeatureEntrySqlParams[4].Value = user.Id;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID, USER_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE, @USER)", InsertFeatureEntrySqlParams));

			// Get new Id back
			return new DbFeatureEntry(context.GetNewId(), feature, details, timeSpent);
		}

		//public static void InsertExceptionEntry(ITransactionContext context, DbFeature feature, FeatureException featureException)
		//{
		//	if (context == null) throw new ArgumentNullException(nameof(context));
		//	if (feature == null) throw new ArgumentNullException(nameof(feature));
		//	if (featureException == null) throw new ArgumentNullException(nameof(featureException));

		//	// Set parameters values
		//	InsertExcetionEntrySqlParams[0].Value = featureException.Exception.Message;
		//	InsertExcetionEntrySqlParams[1].Value = featureException.Exception.StackTrace;
		//	InsertExcetionEntrySqlParams[2].Value = DateTime.Now;
		//	InsertExcetionEntrySqlParams[3].Value = feature.Id;

		//	// Insert the record
		//	context.Execute(new Query(@"INSERT INTO FEATURE_EXCEPTIONS(MESSAGE, STACKTRACE, CREATEDAT, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATEDAT, @FEATURE)", InsertExcetionEntrySqlParams));
		//}

		public static void InsertStepEntry(ITransactionContext context, DbFeatureEntry featureEntry, DbFeatureStep step, FeatureEntryStep entryStep)
		{
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));
			if (step == null) throw new ArgumentNullException(nameof(step));
			if (entryStep == null) throw new ArgumentNullException(nameof(entryStep));

			// Set parameters values
			InsertFeatureStepEntrySqlParams[0].Value = featureEntry.Id;
			InsertFeatureStepEntrySqlParams[1].Value = step.Id;
			InsertFeatureStepEntrySqlParams[2].Value = Convert.ToDecimal(entryStep.TimeSpent.TotalMilliseconds);
			InsertFeatureStepEntrySqlParams[3].Value = entryStep.Details;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams));
		}

		public static long InsertUser(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertUserSqlParams[0].Value = name;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_USERS(NAME) VALUES (@NAME)", InsertUserSqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		public static long? GetUser(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertUserSqlParams[0].Value = name;

			// select the record
			var ids = context.Execute(new Query<long>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", r => r.GetInt64(0), InsertUserSqlParams));
			if (ids.Count > 0)
			{
				return ids[0];
			}
			return null;
		}

		public static long Insert(ITransactionContext context, DbFeatureUser user, FeatureEntryRow row)
		{
			// Set parameters values
			InsertFeatureEntrySqlParams[0].Value = row.TimeSpent;
			InsertFeatureEntrySqlParams[1].Value = row.Details;
			InsertFeatureEntrySqlParams[2].Value = DateTime.Now;
			InsertFeatureEntrySqlParams[3].Value = row.FeatureId;
			InsertFeatureEntrySqlParams[4].Value = user.Id;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID, USER_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE, @USER)", InsertFeatureEntrySqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		public static void Insert(ITransactionContext context, long featureEntryId, FeatureEntryStepRow row)
		{
			// Set parameters values
			InsertFeatureStepEntrySqlParams[0].Value = featureEntryId;
			InsertFeatureStepEntrySqlParams[1].Value = row.FeatureStepId;
			InsertFeatureStepEntrySqlParams[2].Value = row.TimeSpent;
			InsertFeatureStepEntrySqlParams[3].Value = row.Details;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams));
		}
	}
}