using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerAdapter
	{
		private static readonly Query InserChangeQuery =
			new Query(@"INSERT INTO FEATURE_CHANGES(LAST_CHANGED_AT) VALUES (@CHANGED_AT)", new[]
			{
				new QueryParameter(@"CHANGED_AT", DateTime.MinValue),
			});

		private static readonly Query UpdateChangeQuery =
			new Query(@"UPDATE FEATURE_CHANGES SET LAST_CHANGED_AT = @CHANGED_AT", new[]
			{
				new QueryParameter(@"CHANGED_AT", DateTime.MinValue),
			});

		private static readonly Query InserUserQuery =
			new Query(@"INSERT INTO FEATURE_USERS(NAME, REPLICATED_AT, VERSION_ID) VALUES (@NAME, @REPLICATED_AT, @VERSION_ID)",
				new[]
				{
					new QueryParameter(@"NAME", string.Empty),
					new QueryParameter(@"REPLICATED_AT", DateTime.MinValue),
					new QueryParameter(@"@VERSION_ID", 0),
				});

		private static readonly Query InserVersionQuery = new Query(@"INSERT INTO FEATURE_VERSIONS(NAME) VALUES (@NAME)",
			new[]
			{
				new QueryParameter(@"NAME", string.Empty),
			});

		private static readonly Query UpdateUserQuery =
			new Query(@"UPDATE FEATURE_USERS SET REPLICATED_AT = @REPLICATED_AT, VERSION_ID = @VERSION_ID WHERE ID = @ID",
				new[]
				{
					new QueryParameter(@"@ID", 0L),
					new QueryParameter(@"@REPLICATED_AT", DateTime.MinValue),
					new QueryParameter(@"@VERSION_ID", 0),
				});

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

		private static readonly Query InsertExceptionEntryQuery =
			new Query(@"INSERT INTO FEATURE_EXCEPTION_ENTRIES(EXCEPTION_ID, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES (@EXCEPTION, @CREATED_AT, @FEATURE, @USER, @VERSION)",
				new[]
				{
					new QueryParameter(@"@EXCEPTION", 0L),
					new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
					new QueryParameter(@"@FEATURE", 0L),
					new QueryParameter(@"@USER", 0L),
					new QueryParameter(@"@VERSION", 0L),
				});


		private static readonly Query<int> GetUserQuery = new Query<int>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", ReadInt, new[]
			{
				new QueryParameter(@"NAME", string.Empty),
			});

		private static readonly Query<int> GetVersionQuery = new Query<int>(@"SELECT ID FROM FEATURE_VERSIONS WHERE NAME = @NAME", ReadInt, new[]
			{
				new QueryParameter(@"NAME", string.Empty),
			});

		public static Task CreateSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"
CREATE TABLE[FEATURE_CHANGES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[LAST_CHANGED_AT] datetime NOT NULL
)"));

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
	[Feature_Entry_Id] integer NOT NULL, 
	[Feature_Step_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Entry_Id])
		REFERENCES [FEATURE_ENTRIES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Feature_Step_Id])
		REFERENCES [FEATURE_STEPS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE		
)"));

			return Task.FromResult(true);
		}

		public static Task DropSchemaAsync(ITransactionContext context)
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
				@"FEATURE_CHANGES",
				@"FEATURE_EXCEPTIONS"
			})
			{
				context.Execute(new Query(@"DROP TABLE " + name));
			}

			return Task.FromResult(true);
		}

		public static Task<long> GetOrCreateVersionAsync(ITransactionContext context, string version)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (version == null) throw new ArgumentNullException(nameof(version));

			// Set parameters values
			GetVersionQuery.Parameters[0].Value = version;

			// select the record
			var ids = context.Execute(GetVersionQuery);
			if (ids.Count > 0)
			{
				return Task.FromResult(Convert.ToInt64(ids[0]));
			}

			InserVersionQuery.Parameters[0].Value = version;

			return FeatureAdapter.ExecuteInsertAsync(context, InserVersionQuery);
		}

		public static Task<long> GetOrCreateUserAsync(ITransactionContext context, string userName, int versionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			var currentTime = DateTime.Now;

			// Set parameters values
			GetUserQuery.Parameters[0].Value = userName;

			// select the record
			var ids = context.Execute(GetUserQuery);
			if (ids.Count > 0)
			{
				var userId = ids[0];

				UpdateUserQuery.Parameters[0].Value = userId;
				UpdateUserQuery.Parameters[1].Value = currentTime;
				UpdateUserQuery.Parameters[2].Value = versionId;

				context.Execute(UpdateUserQuery);

				return Task.FromResult(Convert.ToInt64(userId));
			}

			InserUserQuery.Parameters[0].Value = userName;
			InserUserQuery.Parameters[1].Value = currentTime;
			InserUserQuery.Parameters[2].Value = versionId;

			return FeatureAdapter.ExecuteInsertAsync(context, InserUserQuery);
		}

		public static Task<Dictionary<string, int>> GetContextsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_CONTEXTS");
		}

		public static Task<Dictionary<string, int>> GetStepsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_STEPS");
		}

		public static Task<Dictionary<string, int>> GetExceptionsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS");
		}

		public static Task<long> InsertFeatureEntryAsync(ITransactionContext context, double timeSpent, string details, DateTime createdAt, int featureId, int userId, int versionId, bool needNewId)
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

			var id = -1L;
			if (needNewId)
			{
				id = context.GetNewId();
			}
			return Task.FromResult(id);
		}

		public static Task InsertExceptionEntryAsync(ITransactionContext context, long exceptionId, DateTime createdAt, long featureId, long userId, long versionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Set parameters values
			InsertExceptionEntryQuery.Parameters[0].Value = exceptionId;
			InsertExceptionEntryQuery.Parameters[1].Value = createdAt;
			InsertExceptionEntryQuery.Parameters[2].Value = featureId;
			InsertExceptionEntryQuery.Parameters[3].Value = userId;
			InsertExceptionEntryQuery.Parameters[4].Value = versionId;

			// Insert the record
			context.Execute(InsertExceptionEntryQuery);

			return Task.FromResult(true);
		}

		public static Task UpdateLastChangedFlagAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			UpdateChangeQuery.Parameters[0].Value = InserChangeQuery.Parameters[0].Value = DateTime.Now;

			var isUpdated = Convert.ToBoolean(context.Execute(UpdateChangeQuery));
			if (!isUpdated)
			{
				context.Execute(InserChangeQuery);
			}

			return Task.FromResult(true);
		}

		private static int ReadInt(IFieldDataReader r)
		{
			return r.GetInt32(0);
		}

		private static Task<Dictionary<string, int>> GetDataMapped(ITransactionContext context, string statement)
		{
			var result = new Dictionary<string, int>(32);

			context.Fill(result, (r, map) => { map.Add(r.GetString(1), r.GetInt32(0)); }, new Query(statement));

			return Task.FromResult(result);
		}
	}
}