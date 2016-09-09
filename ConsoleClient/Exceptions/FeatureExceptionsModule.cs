using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Cchbc.Data;
using Cchbc.Objects;

namespace ConsoleClient.Exceptions
{
	public sealed class ExceptionsCount
	{
		public DateTime DateTime { get; }
		public int Count { get; }

		public ExceptionsCount(DateTime dateTime, int count)
		{
			this.DateTime = dateTime;
			this.Count = count;
		}
	}







	public sealed class TimePeriod
	{
		public TimeSpan TimeOffset { get; }
		public string Name { get; }

		public TimePeriod(TimeSpan timeOffset, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.TimeOffset = timeOffset;
			this.Name = name;
		}
	}

	public sealed class TimePeriodViewModel
	{
		public TimePeriod TimePeriod { get; }
		public string Name { get; }

		public TimePeriodViewModel(TimePeriod timePeriod)
		{
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			this.TimePeriod = timePeriod;
			this.Name = timePeriod.Name;
		}
	}


	public static class FeatureExceptionsDataProvider
	{
		public static IEnumerable<TimePeriod> GetTimePeriods(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return new[]
			{
				new TimePeriod(TimeSpan.FromHours(1), @"Last 1 hour"),
				new TimePeriod(TimeSpan.FromHours(24), @"Last 24 hours"),
				new TimePeriod(TimeSpan.FromDays(7), @"Last 7 days"),
				new TimePeriod(TimeSpan.FromDays(30), @"Last 30 days"),
				new TimePeriod(TimeSpan.MaxValue, @"All time"),
			};
		}

		public static IEnumerable<FeatureVersionRow> GetVersions(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var query = @"SELECT ID, NAME FROM FEATURE_VERSIONS ORDER BY NAME";

			return context.Execute(new Query<FeatureVersionRow>(query, r => new FeatureVersionRow(r.GetInt64(0), r.GetString(1))));
		}

		public static IEnumerable<FeatureExceptionEntry> GetExceptions(ExceptionsDataLoadParams dataLoadParams)
		{
			if (dataLoadParams == null) throw new ArgumentNullException(nameof(dataLoadParams));

			var context = dataLoadParams.Context;

			var query = @"
			SELECT E.ID, E.FEATURE_ID, E.EXCEPTION_ID, E.USER_ID, E.VERSION_ID, E.CREATED_AT
			FROM FEATURE_EXCEPTION_ENTRIES E
			WHERE NOT EXISTS
				(SELECT 1
				 FROM FEATURE_EXCEPTIONS_EXCLUDED EX
				 WHERE E.EXCEPTION_ID = EX.EXCEPTION_ID)
			ORDER BY E.CREATED_AT DESC
			LIMIT @maxEntries";

			var sqlParams = new[]
					{
				new QueryParameter(@"@maxEntries", dataLoadParams.MaxEntries)
			};

			var exceptionEntries = context.Execute(new Query<FeatureExceptionEntryRow>(query, FeatureExceptionEntryRowCreator, sqlParams));
			if (exceptionEntries.Count == 0) return Enumerable.Empty<FeatureExceptionEntry>().ToArray();

			var features = GetFeatures(context, exceptionEntries);
			var exceptions = GetExceptions(context, exceptionEntries);
			var users = GetUsers(context, exceptionEntries);
			var versions = GetVersions(context, exceptionEntries);

			var featureExceptions = new FeatureExceptionEntry[exceptionEntries.Count];
			for (var i = 0; i < exceptionEntries.Count; i++)
			{
				var r = exceptionEntries[i];
				featureExceptions[i] = new FeatureExceptionEntry(r.Id, features[r.FeatureId], exceptions[r.ExceptionId], users[r.UserId], versions[r.VersionId], r.CreatedAt);
			}

			return featureExceptions;
		}

		public static IEnumerable<ExceptionsCount> GetExceptionsCounts(ExceptionsDataLoadParams dataLoadParams)
		{
			if (dataLoadParams == null) throw new ArgumentNullException(nameof(dataLoadParams));

			//TODO : !!!

			return new ExceptionsCount[0];
		}

		private static Dictionary<long, FeatureRow> GetFeatures(ITransactionContext context, List<FeatureExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, FeatureRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<FeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new FeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.FeatureId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;
		}

		private static Dictionary<long, FeatureVersionRow> GetVersions(ITransactionContext context, List<FeatureExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, FeatureVersionRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<FeatureVersionRow>(@"SELECT ID, NAME FROM FEATURE_VERSIONS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new FeatureVersionRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.VersionId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;
		}

		private static Dictionary<long, FeatureExceptionRow> GetExceptions(ITransactionContext context, List<FeatureExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, FeatureExceptionRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<FeatureExceptionRow>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new FeatureExceptionRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.ExceptionId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;
		}

		private static Dictionary<long, FeatureUserRow> GetUsers(ITransactionContext context, List<FeatureExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, FeatureUserRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<FeatureUserRow>(@"SELECT ID, NAME FROM FEATURE_USERS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new FeatureUserRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.UserId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;

			//var sqlParams = new QueryParameter[16];
			//var batch = sqlParams.Length;
			//for (var i = 0; i < batch; i++)
			//{
			//	sqlParams[i] = new QueryParameter(@"p" + i);
			//}

			//var query = new Query<FeatureUserRow>(@"SELECT ID, NAME FROM FEATURE_USERS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new FeatureUserRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			//var ids = rows.Select(r => r.User).Distinct().ToList();

			//for (var i = 0; i < ids.Count / batch; i++)
			//{
			//	var offset = i * batch;
			//	for (var j = 0; j < batch; j++)
			//	{
			//		sqlParams[j].Value = ids[offset + j];
			//	}
			//	context.Fill(values, v => v.Id, query);
			//}

			//var rem = ids.Count % batch;
			//if (rem > 0)
			//{
			//	foreach (var p in sqlParams)
			//	{
			//		p.Value = -1;
			//	}
			//	for (var i = 0; i < ids.Count; i++)
			//	{
			//		sqlParams[i].Value = ids[i];
			//	}
			//	context.Fill(values, v => v.Id, query);
			//}

			//return values;
		}

		private static FeatureExceptionEntryRow FeatureExceptionEntryRowCreator(IFieldDataReader r)
		{
			return new FeatureExceptionEntryRow(r.GetInt64(0), r.GetInt64(1), r.GetInt64(2), r.GetInt64(3), r.GetInt64(4), r.GetDateTime(5));
		}

		private static QueryParameter[] CreateBatchParams(int batchSize)
		{
			var sqlParams = new QueryParameter[batchSize];

			for (var i = 0; i < sqlParams.Length; i++)
			{
				sqlParams[i] = new QueryParameter(@"p" + i);
			}

			return sqlParams;
		}
	}






	public sealed class ExceptionsDataLoadParams
	{
		public ITransactionContext Context { get; }
		public FeatureVersion Version { get; }
		public TimePeriod TimePeriod { get; }
		public int MaxEntries { get; }

		public ExceptionsDataLoadParams(ITransactionContext context, FeatureVersion version, TimePeriod timePeriod, int maxEntries)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (version == null) throw new ArgumentNullException(nameof(version));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			this.Context = context;
			this.Version = version;
			this.TimePeriod = timePeriod;
			this.MaxEntries = maxEntries;
		}
	}

	public sealed class FeatureExceptionsSettings
	{
		public int MaxExceptionEntries { get; } = 10;
	}

	public sealed class FeatureExceptionsViewModel : ViewModel
	{
		private ExceptionsDataLoadParams ExceptionsDataLoadParams(ITransactionContext context)
		{
			return new ExceptionsDataLoadParams(context, this.Version, this.TimePeriod.TimePeriod, this.Settings.MaxExceptionEntries);
		}

		private bool _isVersionsLoading;
		public bool IsVersionsLoading
		{
			get { return _isVersionsLoading; }
			set { this.SetField(ref _isVersionsLoading, value); }
		}
		private FeatureVersion _version;
		public FeatureVersion Version
		{
			get { return _version; }
			set
			{
				this.SetField(ref _version, value);
				this.LoadCurrentExceptions();
			}
		}
		public ObservableCollection<FeatureVersion> Versions { get; } = new ObservableCollection<FeatureVersion>();

		private bool _isTimePeriodsLoading;
		public bool IsTimePeriodsLoading
		{
			get { return _isTimePeriodsLoading; }
			set { this.SetField(ref _isTimePeriodsLoading, value); }
		}
		private TimePeriodViewModel _timePeriod;
		public TimePeriodViewModel TimePeriod
		{
			get { return _timePeriod; }
			set
			{
				this.SetField(ref _timePeriod, value);
				this.LoadCurrentExceptions();
			}
		}
		public ObservableCollection<TimePeriodViewModel> TimePeriods { get; } = new ObservableCollection<TimePeriodViewModel>();

		private bool _isExceptionsLoading;
		public bool IsExceptionsLoading
		{
			get { return _isExceptionsLoading; }
			set { this.SetField(ref _isExceptionsLoading, value); }
		}
		public ObservableCollection<FeatureExceptionEntry> Exceptions { get; } = new ObservableCollection<FeatureExceptionEntry>();
		private bool _isExceptionsCountsLoading;
		public bool IsExceptionsCountsLoading
		{
			get { return _isExceptionsCountsLoading; }
			set { this.SetField(ref _isExceptionsCountsLoading, value); }
		}
		public ObservableCollection<ExceptionsCount> ExceptionsCounts { get; } = new ObservableCollection<ExceptionsCount>();

		public ITransactionContextCreator ContextCreator { get; }
		public Func<ExceptionsDataLoadParams, IEnumerable<FeatureExceptionEntry>> ExceptionsProvider { get; private set; }
		public Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> ExceptionsCountProvider { get; private set; }
		public FeatureExceptionsSettings Settings { get; }

		public FeatureExceptionsViewModel(ITransactionContextCreator contextCreator, FeatureExceptionsSettings settings)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			this.ContextCreator = contextCreator;
			this.Settings = settings;
		}

		public void Load(
			Func<ITransactionContext, IEnumerable<TimePeriod>> timePeriodsProvider,
			Func<ITransactionContext, IEnumerable<FeatureVersionRow>> versionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<FeatureExceptionEntry>> exceptionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> exceptionsCountProvider)
		{
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (exceptionsCountProvider == null) throw new ArgumentNullException(nameof(exceptionsCountProvider));
			if (timePeriodsProvider == null) throw new ArgumentNullException(nameof(timePeriodsProvider));

			this.ExceptionsProvider = exceptionsProvider;
			this.ExceptionsCountProvider = exceptionsCountProvider;

			using (var context = this.ContextCreator.Create())
			{
				this.LoadPeriods(context, timePeriodsProvider);
				this.LoadVersions(context, versionsProvider);

				var hasData = this.TimePeriods.Count > 0 && this.Versions.Count > 0;
				if (hasData)
				{
					this.LoadExceptions(context, this.ExceptionsProvider);
					this.LoadExceptionsCounts(context, this.ExceptionsCountProvider);
				}

				context.Complete();
			}
		}

		private void LoadPeriods(ITransactionContext context, Func<ITransactionContext, IEnumerable<TimePeriod>> timePeriodsProvider)
		{
			this.TimePeriods.Clear();

			foreach (var timePeriod in timePeriodsProvider(context))
			{
				this.TimePeriods.Add(new TimePeriodViewModel(timePeriod));
			}

			// Set the field to avoid reloading exceptions
			_timePeriod = this.TimePeriods.FirstOrDefault();
		}

		private void LoadVersions(ITransactionContext context, Func<ITransactionContext, IEnumerable<FeatureVersionRow>> versionsProvider)
		{
			this.Versions.Clear();

			foreach (var featureVersion in versionsProvider(context))
			{
				this.Versions.Add(new FeatureVersion(featureVersion));
			}

			// Set the field to avoid reloading exceptions
			_version = this.Versions.FirstOrDefault();
		}

		private void LoadExceptions(ITransactionContext context, Func<ExceptionsDataLoadParams, IEnumerable<FeatureExceptionEntry>> exceptionsProvider)
		{
			this.Exceptions.Clear();

			foreach (var featureException in exceptionsProvider(this.ExceptionsDataLoadParams(context)))
			{
				this.Exceptions.Add(featureException);
			}
		}

		private void LoadExceptionsCounts(ITransactionContext context, Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> exceptionsCountProvider)
		{
			this.ExceptionsCounts.Clear();

			foreach (var exceptionsCount in exceptionsCountProvider(this.ExceptionsDataLoadParams(context)))
			{
				this.ExceptionsCounts.Add(exceptionsCount);
			}
		}

		private void LoadCurrentExceptions()
		{
			this.Exceptions.Clear();

			using (var ctx = this.ContextCreator.Create())
			{
				this.LoadExceptions(ctx, this.ExceptionsProvider);

				ctx.Complete();
			}
		}
	}
}