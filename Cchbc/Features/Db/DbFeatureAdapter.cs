using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
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
		}

		public static List<DbContext> GetContexts(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(new Query<DbContext>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbFeatureContextCreator));
		}

		public static List<DbFeatureStep> GetSteps(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(new Query<DbFeatureStep>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator));
		}

		public static List<DbFeature> GetFeatures(ITransactionContext context, Dictionary<string, DbContext> contexts)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));

			var features = context.Execute(new Query<DbFeature>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureCreator));
			if (features.Count == 0) return features;

			var contextsById = new Dictionary<long, DbContext>();
			foreach (var dbContext in contexts.Values)
			{
				contextsById.Add(dbContext.Id, dbContext);
			}

			var mappedFeatures = new List<DbFeature>(features.Count);
			foreach (var feature in features)
			{
				mappedFeatures.Add(new DbFeature(feature.Id, feature.Name, contextsById[feature.Context.Id]));
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

		public static long InsertFeature(ITransactionContext context, string name, long dbContextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureSqlParams[0].Value = name;
			InsertFeatureSqlParams[1].Value = dbContextId;

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", InsertFeatureSqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static DbFeature DbFeatureCreator(IFieldDataReader r)
		{
			return new DbFeature(r.GetInt64(0), r.GetString(1), new DbContext(r.GetInt64(2), string.Empty));
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