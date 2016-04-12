using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db.Server
{
//	public sealed class DbFeatureServerAdapter
//	{
//		private static readonly QueryParameter[] InsertContextSqlParams =
//		{
//			new QueryParameter(@"NAME", string.Empty),
//		};

//		private static readonly QueryParameter[] InsertStepSqlParams =
//		{
//			new QueryParameter(@"NAME", string.Empty),
//		};

//		private static readonly QueryParameter[] InsertFeatureSqlParams =
//		{
//			new QueryParameter(@"@NAME", string.Empty),
//			new QueryParameter(@"@CONTEXT", 0L),
//		};

//		private static readonly QueryParameter[] InsertFeatureEntrySqlParams =
//		{
//			new QueryParameter(@"@TIMESPENT", 0M),
//			new QueryParameter(@"@DETAILS", string.Empty),
//			new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
//			new QueryParameter(@"@FEATURE", 0L),
//		};

//		private static readonly QueryParameter[] InsertExcetionEntrySqlParams =
//		{
//			new QueryParameter(@"@MESSAGE", string.Empty),
//			new QueryParameter(@"@STACKTRACE", string.Empty),
//			new QueryParameter(@"@CREATEDAT", DateTime.MinValue),
//			new QueryParameter(@"@FEATURE", 0L),
//		};

//		private static readonly QueryParameter[] InsertFeatureStepEntrySqlParams =
//		{
//			new QueryParameter(@"@ENTRY", 0L),
//			new QueryParameter(@"@STEP", 0L),
//			new QueryParameter(@"@TIMESPENT", 0M),
//			new QueryParameter(@"@DETAILS", string.Empty),
//		};

//		public void CreateSchema(ITransactionContext context)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));

//			context.Execute(new Query(@"
//CREATE TABLE[FEATURE_USERS] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
//	[Name] nvarchar(254) NOT NULL
//)"));

//			context.Execute(new Query(@"
//CREATE TABLE[FEATURE_CONTEXTS] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
//	[Name] nvarchar(254) NOT NULL
//)"));

//			context.Execute(new Query(@"
//CREATE TABLE[FEATURE_STEPS] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
//	[Name] nvarchar(254) NOT NULL
//)"));

//			context.Execute(new Query(@"
//CREATE TABLE [FEATURES] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[Name] nvarchar(254) NOT NULL, 
//	[Context_Id] integer NOT NULL, 
//	FOREIGN KEY ([Context_Id])
//		REFERENCES [FEATURE_CONTEXTS] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE
//)"));

//			context.Execute(new Query(@"
//CREATE TABLE [FEATURE_ENTRIES] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[TimeSpent] decimal(38, 0) NOT NULL, 
//	[Details] nvarchar(254) NULL, 
//	[CreatedAt] datetime NOT NULL, 
//	[Feature_Id] integer NOT NULL, 
//	FOREIGN KEY ([Feature_Id])
//		REFERENCES [FEATURES] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE
//)"));

//			context.Execute(new Query(@"
//CREATE TABLE [EXCEPTION_ENTRIES] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[Message] nvarchar(254) NOT NULL, 
//	[StackTrace] nvarchar(254) NOT NULL, 
//	[CreatedAt] datetime NOT NULL, 
//	[Feature_Id] integer NOT NULL, 
//	FOREIGN KEY ([Feature_Id])
//		REFERENCES [FEATURES] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE
//)"));

//			context.Execute(new Query(@"
//CREATE TABLE [FEATURE_ENTRY_STEPS] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[TimeSpent] decimal(38, 0) NOT NULL, 
//	[Details] nvarchar(254) NULL, 
//	[Feature_Entry_Id] integer NOT NULL, 
//	[Feature_Step_Id] integer NOT NULL, 
//	FOREIGN KEY ([Feature_Entry_Id])
//		REFERENCES [FEATURE_ENTRIES] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE
//	FOREIGN KEY ([Feature_Step_Id])
//		REFERENCES [FEATURE_STEPS] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE		
//)"));
//		}

//		public void DropSchema(ITransactionContext context)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));

//			try
//			{
//				context.Execute(new Query(@"DROP TABLE FEATURE_ENTRY_STEPS"));
//				context.Execute(new Query(@"DROP TABLE EXCEPTION_ENTRIES"));
//				context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
//				context.Execute(new Query(@"DROP TABLE FEATURES"));
//				context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
//				context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
//			}
//			catch { }
//		}

//		public List<DbContext> GetContexts(ITransactionContext context)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));

//			return context.Execute(new Query<DbContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbFeatureContextCreator));
//		}

//		public List<DbFeatureStep> GetSteps(ITransactionContext context)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));

//			return context.Execute(new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator));
//		}

//		public List<DbFeature> GetFeatures(ITransactionContext context)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));

//			return context.Execute(new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureCreator));
//		}

//		public DbContext InsertContext(ITransactionContext context, string name)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));
//			if (name == null) throw new ArgumentNullException(nameof(name));

//			// Set parameters values
//			InsertContextSqlParams[0].Value = name;

//			// Insert the record
//			context.Execute(new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", InsertContextSqlParams));

//			// Get new Id back
//			return new DbContext(context.GetNewId(), name);
//		}

//		public DbFeature InsertFeature(ITransactionContext transactionContext, DbContext context, string name)
//		{
//			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
//			if (context == null) throw new ArgumentNullException(nameof(context));
//			if (name == null) throw new ArgumentNullException(nameof(name));

//			// Set parameters values
//			InsertFeatureSqlParams[0].Value = name;
//			InsertFeatureSqlParams[1].Value = context.Id;

//			// Insert the record
//			transactionContext.Execute(new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", InsertFeatureSqlParams));

//			// Get new Id back
//			return new DbFeature(transactionContext.GetNewId(), name, context.Id);
//		}

//		public DbFeatureEntry InsertFeatureEntry(ITransactionContext context, DbFeature feature, FeatureEntry featureEntry)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));
//			if (feature == null) throw new ArgumentNullException(nameof(feature));
//			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));

//			var timeSpent = featureEntry.TimeSpent;
//			var details = featureEntry.Details;

//			// Set parameters values
//			InsertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
//			InsertFeatureEntrySqlParams[1].Value = details;
//			InsertFeatureEntrySqlParams[2].Value = DateTime.Now;
//			InsertFeatureEntrySqlParams[3].Value = feature.Id;

//			// Insert the record
//			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE)", InsertFeatureEntrySqlParams));

//			// Get new Id back
//			return new DbFeatureEntry(context.GetNewId(), feature, details, timeSpent);
//		}

//		public DbFeatureStep InsertStep(ITransactionContext context, string name)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));
//			if (name == null) throw new ArgumentNullException(nameof(name));

//			// Set parameters values
//			InsertStepSqlParams[0].Value = name;

//			// Insert the record
//			context.Execute(new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", InsertStepSqlParams));

//			// Get new Id back
//			return new DbFeatureStep(context.GetNewId(), name);
//		}

//		public void InsertExceptionEntry(ITransactionContext context, DbFeature feature, ExceptionEntry exceptionEntry)
//		{
//			if (context == null) throw new ArgumentNullException(nameof(context));
//			if (feature == null) throw new ArgumentNullException(nameof(feature));
//			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

//			// Set parameters values
//			InsertExcetionEntrySqlParams[0].Value = exceptionEntry.Exception.Message;
//			InsertExcetionEntrySqlParams[1].Value = exceptionEntry.Exception.StackTrace;
//			InsertExcetionEntrySqlParams[2].Value = DateTime.Now;
//			InsertExcetionEntrySqlParams[3].Value = feature.Id;

//			// Insert the record
//			context.Execute(new Query(@"INSERT INTO EXCEPTION_ENTRIES(MESSAGE, STACKTRACE, CREATEDAT, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATEDAT, @FEATURE)", InsertExcetionEntrySqlParams));
//		}

//		public void InsertStepEntry(ITransactionContext context, DbFeatureEntry featureEntry, DbFeatureStep step, FeatureEntryStep entryStep)
//		{
//			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));
//			if (step == null) throw new ArgumentNullException(nameof(step));
//			if (entryStep == null) throw new ArgumentNullException(nameof(entryStep));

//			// Set parameters values
//			InsertFeatureStepEntrySqlParams[0].Value = featureEntry.Id;
//			InsertFeatureStepEntrySqlParams[1].Value = step.Id;
//			InsertFeatureStepEntrySqlParams[2].Value = Convert.ToDecimal(entryStep.TimeSpent.TotalMilliseconds);
//			InsertFeatureStepEntrySqlParams[3].Value = entryStep.Details;

//			// Insert the record
//			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams));
//		}

//		private static DbFeature DbFeatureCreator(IFieldDataReader r)
//		{
//			return new DbFeature(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
//		}

//		private static DbFeatureStep DbFeatureStepCreator(IFieldDataReader r)
//		{
//			return new DbFeatureStep(r.GetInt64(0), r.GetString(1));
//		}

//		private static DbContext DbFeatureContextCreator(IFieldDataReader r)
//		{
//			return new DbContext(r.GetInt64(0), r.GetString(1));
//		}
//	}
}