using System;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public static class DbFeatureClientAdapter
	{
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
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NULL, 
	[CreatedAt] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [EXCEPTION_ENTRIES] (
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
			context.Execute(new Query(@"DROP TABLE EXCEPTION_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
		}
		
		public static DbFeatureEntry InsertFeatureEntry(ITransactionContext context, DbFeature feature, FeatureEntry featureEntry)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));

			var timeSpent = featureEntry.TimeSpent;
			var details = featureEntry.Details;

			// Set parameters values
			InsertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
			InsertFeatureEntrySqlParams[1].Value = details;
			InsertFeatureEntrySqlParams[2].Value = DateTime.Now;
			InsertFeatureEntrySqlParams[3].Value = feature.Id;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE)", InsertFeatureEntrySqlParams));

			// Get new Id back
			return new DbFeatureEntry(context.GetNewId(), feature, details, timeSpent);
		}

		public static void InsertExceptionEntry(ITransactionContext context, DbFeature feature, ExceptionEntry exceptionEntry)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

			// Set parameters values
			InsertExcetionEntrySqlParams[0].Value = exceptionEntry.Exception.Message;
			InsertExcetionEntrySqlParams[1].Value = exceptionEntry.Exception.StackTrace;
			InsertExcetionEntrySqlParams[2].Value = DateTime.Now;
			InsertExcetionEntrySqlParams[3].Value = feature.Id;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO EXCEPTION_ENTRIES(MESSAGE, STACKTRACE, CREATEDAT, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATEDAT, @FEATURE)", InsertExcetionEntrySqlParams));
		}

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
	}
}