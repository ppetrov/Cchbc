using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerAdapter
	{
		private static readonly Query InsertServerFeatureEntryQuery =
			new Query(
				@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES (@TIMESPENT, @DETAILS, @CREATED_AT, @FEATURE, @USER, @VERSION)",
				new[]
				{
					new QueryParameter(@"@TIMESPENT", 0M),
					new QueryParameter(@"@DETAILS", string.Empty),
					new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
					new QueryParameter(@"@FEATURE", 0L),
					new QueryParameter(@"@USER", 0L),
					new QueryParameter(@"@VERSION", 0L),
				});

		public static void CreateSchemaAsync(ITransactionContext context)
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
CREATE TABLE[FEATURE_EXCEPTIONS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Contents] nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE[FEATURE_VERSIONS] (
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
	[Name] nvarchar(254) NOT NULL,
	[Replicated_At] datetime NOT NULL,
	[Version_Id] integer NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_EXCEPTION_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Exception_Id] integer NOT NULL, 
	[Created_At] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	[User_Id] integer NOT NULL, 
	[Version_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([User_Id])
		REFERENCES [FEATURE_USERS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Version_Id])
		REFERENCES [FEATURE_VERSIONS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Exception_Id])
		REFERENCES [FEATURE_EXCEPTIONS] ([Id])
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
	[Version_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([User_Id])
		REFERENCES [FEATURE_USERS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Version_Id])
		REFERENCES [FEATURE_VERSIONS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_STEP_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Level] integer NOT NULL, 
	[Feature_Entry_Id] integer NOT NULL, 
	[Feature_Step_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Entry_Id])
		REFERENCES [FEATURE_ENTRIES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Feature_Step_Id])
		REFERENCES [FEATURE_STEPS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE		
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_EXCEPTIONS_EXCLUDED] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Exception_Id] bigint NOT NULL, 
	FOREIGN KEY ([Exception_Id])
		REFERENCES [FEATURE_EXCEPTIONS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
		}

		public static void DropSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			foreach (var name in new[]
			{
				@"FEATURE_STEP_ENTRIES",
				@"FEATURE_EXCEPTION_ENTRIES",
				@"FEATURE_ENTRIES",
				@"FEATURE_USERS",
				@"FEATURES",
				@"FEATURE_STEPS",
				@"FEATURE_CONTEXTS",
				@"FEATURE_VERSIONS",
				@"FEATURE_EXCEPTIONS"
			})
			{
				context.Execute(new Query(@"DROP TABLE " + name));
			}
		}

		public static long InsertVersionAsync(ITransactionContext context, string version)
		{
			return FeatureAdapter.ExecuteInsert(context, new Query(@"INSERT INTO FEATURE_VERSIONS(NAME) VALUES (@NAME)", new[] { new QueryParameter(@"NAME", version), }));
		}

		public static long InsertUserAsync(ITransactionContext context, string userName, long versionId)
		{
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", userName),
				new QueryParameter(@"REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION_ID", versionId),
			};

			return FeatureAdapter.ExecuteInsert(context, new Query(@"INSERT INTO FEATURE_USERS(NAME, REPLICATED_AT, VERSION_ID) VALUES (@NAME, @REPLICATED_AT, @VERSION_ID)", sqlParams));
		}

		public static void UpdateUserAsync(ITransactionContext context, long userId, long versionId)
		{
			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", userId),
				new QueryParameter(@"@REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION", versionId),
			};

			context.Execute(new Query(@"UPDATE FEATURE_USERS SET REPLICATED_AT = @REPLICATED_AT, VERSION_ID = @VERSION WHERE ID = @ID", sqlParams));
		}

		public static Dictionary<string, long> GetUsersAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_USERS");
		}

		public static Dictionary<string, long> GetVersionsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_VERSIONS");
		}

		public static Dictionary<string, long> GetContextsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_CONTEXTS");
		}

		public static Dictionary<string, long> GetStepsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_STEPS");
		}

		public static Dictionary<string, long> GetExceptionsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS");
		}

		public static long InsertFeatureEntryAsync(ITransactionContext context, double timeSpent, string details, DateTime createdAt, long featureId, long userId, long versionId, bool needNewId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertServerFeatureEntryQuery.Parameters[0].Value = Convert.ToDecimal(timeSpent);
			InsertServerFeatureEntryQuery.Parameters[1].Value = details;
			InsertServerFeatureEntryQuery.Parameters[2].Value = createdAt;
			InsertServerFeatureEntryQuery.Parameters[3].Value = featureId;
			InsertServerFeatureEntryQuery.Parameters[4].Value = userId;
			InsertServerFeatureEntryQuery.Parameters[5].Value = versionId;

			context.Execute(InsertServerFeatureEntryQuery);

			if (needNewId)
			{
				return context.GetNewId();
			}
			return -1L;
		}

		public static void InsertStepEntriesAsync(ITransactionContext context, IEnumerable<DbFeatureEntryStepRow> entryStepRows, Dictionary<long, long> featureEntriesMap, Dictionary<int, long> stepsMap)
		{
			var buffer = new StringBuilder(@"INSERT INTO FEATURE_STEP_ENTRIES(TIMESPENT, LEVEL, FEATURE_ENTRY_ID, FEATURE_STEP_ID) VALUES ");

			var addComma = false;
			foreach (var s in entryStepRows)
			{
				if (addComma)
				{
					buffer.Append(',');
				}

				buffer.Append('(');
				buffer.Append(s.TimeSpent);
				buffer.Append(',');
				buffer.Append(s.Level);
				buffer.Append(',');
				buffer.Append(featureEntriesMap[s.FeatureEntryId]);
				buffer.Append(',');
				buffer.Append(stepsMap[s.FeatureStepId]);
				buffer.Append(')');

				addComma = true;
			}

			context.Execute(new Query(buffer.ToString()));
		}

		public static void InsertExceptionEntryAsync(ITransactionContext context, IEnumerable<DbFeatureExceptionEntryRow> exceptionEntryRows, long userId, long versionId, Dictionary<int, long> exceptionsMap, Dictionary<int, long> featuresMap)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var buffer = new StringBuilder(@"INSERT INTO FEATURE_EXCEPTION_ENTRIES(EXCEPTION_ID, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES ");

			var addComma = false;
			foreach (var r in exceptionEntryRows)
			{
				if (addComma)
				{
					buffer.Append(',');
				}

				buffer.Append('(');
				buffer.Append(exceptionsMap[r.ExceptionId]);
				buffer.Append(',');
				buffer.Append('\'');
				buffer.Append(r.CreatedAt.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"));
				buffer.Append('\'');
				buffer.Append(',');
				buffer.Append(featuresMap[r.FeatureId]);
				buffer.Append(',');
				buffer.Append(userId);
				buffer.Append(',');
				buffer.Append(versionId);
				buffer.Append(')');

				addComma = true;
			}

			context.Execute(new Query(buffer.ToString()));
		}

		private static Dictionary<string, long> GetDataMapped(ITransactionContext context, string statement)
		{
			var result = new Dictionary<string, long>(32);

			context.Fill(result, (r, map) => { map.Add(r.GetString(1), r.GetInt64(0)); }, new Query(statement));

			return result;
		}
	}
}