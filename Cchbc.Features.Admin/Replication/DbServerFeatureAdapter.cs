using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.Replication
{
    public static class DbServerFeatureAdapter
    {
        private static readonly QueryParameter[] NameSqlParams =
        {
            new QueryParameter(@"NAME", string.Empty),
        };

        private static readonly QueryParameter[] InsertFeatureEntrySqlParams =
        {
            new QueryParameter(@"@TIMESPENT", 0M),
            new QueryParameter(@"@DETAILS", string.Empty),
            new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
            new QueryParameter(@"@FEATURE", 0L),
            new QueryParameter(@"@USER", 0L),
        };

        private static readonly QueryParameter[] InsertExcetionEntrySqlParams =
        {
            new QueryParameter(@"@MESSAGE", string.Empty),
            new QueryParameter(@"@STACKTRACE", string.Empty),
            new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
            new QueryParameter(@"@FEATURE", 0L),
            new QueryParameter(@"@USER", 0L),
        };

        private static readonly Query InserUserQuery = new Query(@"INSERT INTO FEATURE_USERS(NAME) VALUES (@NAME)", NameSqlParams);
        private static readonly Query InsertServerFeatureEntryQuery = new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID, USER_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATED_AT, @FEATURE, @USER)", InsertFeatureEntrySqlParams);
        private static readonly Query InsertExceptionQuery = new Query(@"INSERT INTO FEATURE_EXCEPTIONS(MESSAGE, STACKTRACE, CREATED_AT, FEATURE_ID, USER_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATED_AT, @FEATURE, @USER)", InsertExcetionEntrySqlParams);

        private static readonly Query<DbFeatureEntryRow> GetFeatureEntriesQuery = new Query<DbFeatureEntryRow>(@"SELECT ID, TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
        private static readonly Query<DbFeatureEntryStepRow> GetFeatureEntryStepsQuery = new Query<DbFeatureEntryStepRow>(@"SELECT TIMESPENT, DETAILS, FEATURE_ENTRY_ID, FEATURE_STEP_ID FROM FEATURE_ENTRY_STEPS", EntryStepRowCreator);
        private static readonly Query<DbExceptionRow> GetExceptionsQuery = new Query<DbExceptionRow>(@"SELECT MESSAGE, STACKTRACE, CREATED_AT, FEATURE_ID FROM FEATURE_EXCEPTIONS", DbExceptionRowCreator);
        private static readonly Query<long> GetUserQuery = new Query<long>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", r => r.GetInt64(0), NameSqlParams);

        public static void CreateSchema(ITransactionContext context)
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

            context.Execute(new Query(@"
CREATE TABLE[FEATURE_USERS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)"));

            context.Execute(new Query(@"
CREATE TABLE [FEATURE_EXCEPTIONS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Message] nvarchar(254) NOT NULL, 
	[StackTrace] nvarchar(254) NOT NULL, 
	[Created_At] datetime NOT NULL, 
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
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NULL, 
	[Created_At] datetime NOT NULL, 
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
            context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
            context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
            context.Execute(new Query(@"DROP TABLE FEATURE_USERS"));
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

            return DbFeatureAdapter.GetContexts(context);
        }

        public static List<DbFeatureStepRow> GetSteps(ITransactionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DbFeatureAdapter.GetSteps(context);
        }

        public static List<DbFeatureRow> GetFeatures(ITransactionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return DbFeatureAdapter.GetFeatures(context);
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

        public static List<DbExceptionRow> GetExceptions(ITransactionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.Execute(GetExceptionsQuery);
        }

        public static long InsertContext(ITransactionContext context, string name)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (name == null) throw new ArgumentNullException(nameof(name));

            return DbFeatureAdapter.InsertContext(context, name);
        }

        public static long InsertStep(ITransactionContext context, string name)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (name == null) throw new ArgumentNullException(nameof(name));

            return DbFeatureAdapter.InsertStep(context, name);
        }

        public static long InsertFeature(ITransactionContext context, string name, long contextId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (name == null) throw new ArgumentNullException(nameof(name));

            return DbFeatureAdapter.InsertFeature(context, name, contextId);
        }

        public static long InsertFeatureEntry(ITransactionContext context, long userId, long featureId, string details, decimal timeSpent, DateTime createdAt)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (details == null) throw new ArgumentNullException(nameof(details));

            // Set parameters values
            InsertFeatureEntrySqlParams[0].Value = timeSpent;
            InsertFeatureEntrySqlParams[1].Value = details;
            InsertFeatureEntrySqlParams[2].Value = createdAt;
            InsertFeatureEntrySqlParams[3].Value = featureId;
            InsertFeatureEntrySqlParams[4].Value = userId;

            return ExecureInsert(context, InsertServerFeatureEntryQuery);
        }

        public static void InsertExceptionEntry(ITransactionContext context, long userId, long featureId, DbExceptionRow exceptionRow)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (exceptionRow == null) throw new ArgumentNullException(nameof(exceptionRow));

            // Set parameters values
            InsertExcetionEntrySqlParams[0].Value = exceptionRow.Message;
            InsertExcetionEntrySqlParams[1].Value = exceptionRow.StackTrace;
            InsertExcetionEntrySqlParams[2].Value = exceptionRow.CreatedAt;
            InsertExcetionEntrySqlParams[3].Value = featureId;
            InsertExcetionEntrySqlParams[4].Value = userId;

            // Insert the record
            context.Execute(InsertExceptionQuery);
        }

        public static void InsertStepEntry(ITransactionContext context, long featureEntryId, long stepId, decimal timeSpent, string details)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DbFeatureAdapter.InsertStepEntry(context, featureEntryId, stepId, timeSpent, details);
        }

        private static DbExceptionRow DbExceptionRowCreator(IFieldDataReader r)
        {
            return new DbExceptionRow(r.GetString(0), r.GetString(1), r.GetDateTime(2), r.GetInt64(3));
        }

        private static DbFeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
        {
            return new DbFeatureEntryRow(r.GetInt64(0), r.GetDecimal(1), r.GetString(2), r.GetDateTime(3), r.GetInt64(4));
        }

        private static DbFeatureEntryStepRow EntryStepRowCreator(IFieldDataReader r)
        {
            return new DbFeatureEntryStepRow(r.GetDecimal(0), r.GetString(1), r.GetInt64(2), r.GetInt64(3));
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