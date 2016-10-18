using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerAdapter
	{
		private static readonly string SqliteFullDateTimeFormat = @"yyyy-MM-dd HH:mm:ss.fffffff";

		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_CONTEXTS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_STEPS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTIONS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Contents nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_VERSIONS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Name nvarchar(254) NOT NULL, 
	Context_Id integer NOT NULL, 
	FOREIGN KEY (Context_Id)
		REFERENCES FEATURE_CONTEXTS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_USERS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL,
	Replicated_At datetime NOT NULL,
	Version_Id integer NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTION_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Exception_Id integer NOT NULL, 
	Created_At datetime NOT NULL, 
	Feature_Id integer NOT NULL, 
	User_Id integer NOT NULL, 
	Version_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (User_Id)
		REFERENCES FEATURE_USERS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Version_Id)
		REFERENCES FEATURE_VERSIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Exception_Id)
		REFERENCES FEATURE_EXCEPTIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Details nvarchar(254) NULL, 
	Created_At datetime NOT NULL, 
	Feature_Id integer NOT NULL, 
	User_Id integer NOT NULL, 
	Version_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (User_Id)
		REFERENCES FEATURE_USERS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Version_Id)
		REFERENCES FEATURE_VERSIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTIONS_EXCLUDED (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Exception_Id bigint NOT NULL, 
	FOREIGN KEY (Exception_Id)
		REFERENCES FEATURE_EXCEPTIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
		}

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			foreach (var name in new[]
			{
				@"FEATURE_EXCEPTIONS_EXCLUDED",
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

		public static long InsertVersion(ITransactionContext context, string version)
		{
			return FeatureAdapter.ExecuteInsert(context, new Query(@"INSERT INTO FEATURE_VERSIONS(NAME) VALUES (@NAME)", new[] { new QueryParameter(@"NAME", version), }));
		}

		public static long InsertUser(ITransactionContext context, string userName, long versionId)
		{
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", userName),
				new QueryParameter(@"REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION_ID", versionId),
			};

			return FeatureAdapter.ExecuteInsert(context, new Query(@"INSERT INTO FEATURE_USERS(NAME, REPLICATED_AT, VERSION_ID) VALUES (@NAME, @REPLICATED_AT, @VERSION_ID)", sqlParams));
		}

		public static void UpdateUser(ITransactionContext context, long userId, long versionId)
		{
			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", userId),
				new QueryParameter(@"@REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION", versionId),
			};

			context.Execute(new Query(@"UPDATE FEATURE_USERS SET REPLICATED_AT = @REPLICATED_AT, VERSION_ID = @VERSION WHERE ID = @ID", sqlParams));
		}

		public static Dictionary<string, long> GetUsers(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_USERS");
		}

		public static Dictionary<string, long> GetVersions(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_VERSIONS");
		}

		public static Dictionary<string, long> GetContexts(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_CONTEXTS");
		}

		public static Dictionary<string, long> GetSteps(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_STEPS");
		}

		public static Dictionary<string, long> GetExceptions(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS");
		}

		public static void InsertExceptionEntry(ITransactionContext context, IEnumerable<DbFeatureExceptionEntryRow> exceptionEntryRows, long userId, long versionId, Dictionary<int, long> exceptionsMap, Dictionary<int, long> featuresMap)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (exceptionEntryRows == null) throw new ArgumentNullException(nameof(exceptionEntryRows));
			if (exceptionsMap == null) throw new ArgumentNullException(nameof(exceptionsMap));
			if (featuresMap == null) throw new ArgumentNullException(nameof(featuresMap));

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
				buffer.Append(r.CreatedAt.ToString(SqliteFullDateTimeFormat));
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

		public static void InsertFeatureEntry(ITransactionContext context, IEnumerable<DbFeatureEntryRow> featureEntryRows, long userId, long versionId, Dictionary<int, long> featuresMap)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (featureEntryRows == null) throw new ArgumentNullException(nameof(featureEntryRows));
			if (featuresMap == null) throw new ArgumentNullException(nameof(featuresMap));

			var buffer = new StringBuilder(@"INSERT INTO FEATURE_ENTRIES(DETAILS, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES ");

			var addComma = false;
			foreach (var e in featureEntryRows)
			{
				if (addComma)
				{
					buffer.Append(',');
				}

				buffer.Append('(');
				buffer.Append('\'');
				buffer.Append(e.Details);
				buffer.Append('\'');
				buffer.Append(',');
				buffer.Append('\'');
				buffer.Append(e.CreatedAt.ToString(SqliteFullDateTimeFormat));
				buffer.Append('\'');
				buffer.Append(',');
				buffer.Append(featuresMap[e.FeatureId]);
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