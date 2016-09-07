using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cchbc.Data;
using Cchbc.Objects;

namespace ConsoleClient
{
	public static class FeatureExceptionsDataProvider
	{
		private sealed class FeatureExceptionRow
		{
			public readonly long Id;
			public readonly long Exception;
			public readonly DateTime CreatedAt;
			public readonly long User;
			public readonly long Version;

			public FeatureExceptionRow(long id, long exception, DateTime createdAt, long user, long version)
			{
				this.Id = id;
				this.Exception = exception;
				this.CreatedAt = createdAt;
				this.User = user;
				this.Version = version;
			}
		}

		public static IEnumerable<TimePeriod> GetTimePeriods()
		{
			return new[]
			{
				new TimePeriod(TimeSpan.FromHours(1), @"Last 1 hour"),
				new TimePeriod(TimeSpan.FromHours(24), @"Last 24 hours"),
				new TimePeriod(TimeSpan.FromDays(7), @"Last 7 days"),
				new TimePeriod(TimeSpan.FromDays(30), @"Last 30 days"),
				new TimePeriod(TimeSpan.MaxValue, @"All time"),
			};
		}

		public static IEnumerable<FeatureVersion> GetVersions(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var query = @"SELECT ID, NAME FROM FEATURE_VERSIONS ORDER BY NAME";

			return context.Execute(new Query<FeatureVersion>(query, r => new FeatureVersion(r.GetInt64(0), r.GetString(1))));
		}

		public static IEnumerable<FeatureException> GetExceptions(ExceptionsDataLoadParams dataLoadParams)
		{
			if (dataLoadParams == null) throw new ArgumentNullException(nameof(dataLoadParams));

			var context = dataLoadParams.Context;

			var query = @"SELECT ID, EXCEPTION_ID, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID FROM FEATURE_EXCEPTION_ENTRIES ORDER BY CREATED_AT DESC LIMIT @maxEntries";

			var sqlParams = new[]
			{
				new QueryParameter(@"@maxEntries", dataLoadParams.MaxEntries)
			};

			var rows =
				context.Execute(new Query<FeatureExceptionRow>(query,
					r => new FeatureExceptionRow(r.GetInt64(0), r.GetInt64(1), r.GetDateTime(2), r.GetInt64(3), r.GetInt64(4)), sqlParams));

			// ~ 10 items
			var exceptions = new Dictionary<long, string>();
			context.Fill(exceptions, (r, m) =>
			{
				m.Add(r.GetInt64(0), r.GetString(1));
			}, new Query(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS"));

			// ~ 10 entries only Max entries (distinct) users
			var users = new Dictionary<long, FeatureUser>();
			context.Fill(users, (r, m) =>
			{
				var id = r.GetInt64(0);
				m.Add(id, new FeatureUser(id, r.GetString(1)));
			}, new Query(@"SELECT ID, NAME FROM FEATURE_USERS"));

			// ~ 10 Max entries (distinct) versions
			var versions = new Dictionary<long, FeatureVersion>();
			context.Fill(versions, (r, m) =>
			{
				var id = r.GetInt64(0);
				m.Add(id, new FeatureVersion(id, r.GetString(1)));
			}, new Query(@"SELECT ID, NAME FROM FEATURE_VERSIONS"));

			var featureExceptions = new FeatureException[rows.Count];
			for (var i = 0; i < rows.Count; i++)
			{
				var r = rows[i];
				featureExceptions[i] = new FeatureException(r.Id, exceptions[r.Exception], r.CreatedAt, users[r.User], versions[r.Version]);
			}

			return featureExceptions;
		}

		public static IEnumerable<ExceptionsCount> GetExceptionsCounts(ExceptionsDataLoadParams dataLoadParams)
		{
			if (dataLoadParams == null) throw new ArgumentNullException(nameof(dataLoadParams));

			//TODO

			return new ExceptionsCount[0];
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
			return new ExceptionsDataLoadParams(context, this.Version.Version, this.TimePeriod.TimePeriod, this.Settings.MaxExceptionEntries);
		}

		private bool _isVersionsLoading;
		public bool IsVersionsLoading
		{
			get { return _isVersionsLoading; }
			set { this.SetField(ref _isVersionsLoading, value); }
		}
		private FeatureVersionViewModel _version;
		public FeatureVersionViewModel Version
		{
			get { return _version; }
			set
			{
				this.SetField(ref _version, value);
				this.LoadCurrentExceptions();
			}
		}
		public ObservableCollection<FeatureVersionViewModel> Versions { get; } = new ObservableCollection<FeatureVersionViewModel>();

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
		public ObservableCollection<FeatureExceptionViewModel> Exceptions { get; } = new ObservableCollection<FeatureExceptionViewModel>();
		private bool _isExceptionsCountsLoading;
		public bool IsExceptionsCountsLoading
		{
			get { return _isExceptionsCountsLoading; }
			set { this.SetField(ref _isExceptionsCountsLoading, value); }
		}
		public ObservableCollection<ExceptionsCount> ExceptionsCounts { get; } = new ObservableCollection<ExceptionsCount>();

		public ITransactionContextCreator ContextCreator { get; }
		public Func<ExceptionsDataLoadParams, IEnumerable<FeatureException>> ExceptionsProvider { get; private set; }
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
			Func<IEnumerable<TimePeriod>> timePeriodsProvider,
			Func<ITransactionContext, IEnumerable<FeatureVersion>> versionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<FeatureException>> exceptionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> exceptionsCountProvider)
		{
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (exceptionsCountProvider == null) throw new ArgumentNullException(nameof(exceptionsCountProvider));
			if (timePeriodsProvider == null) throw new ArgumentNullException(nameof(timePeriodsProvider));

			this.ExceptionsProvider = exceptionsProvider;
			this.ExceptionsCountProvider = exceptionsCountProvider;

			this.LoadPeriods(timePeriodsProvider);

			using (var context = this.ContextCreator.Create())
			{
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

		private void LoadPeriods(Func<IEnumerable<TimePeriod>> timePeriodsProvider)
		{
			this.TimePeriods.Clear();

			foreach (var timePeriod in timePeriodsProvider())
			{
				this.TimePeriods.Add(new TimePeriodViewModel(timePeriod));
			}

			// Set the field to avoid reloading exceptions
			_timePeriod = this.TimePeriods.FirstOrDefault();
		}

		private void LoadVersions(ITransactionContext context, Func<ITransactionContext, IEnumerable<FeatureVersion>> versionsProvider)
		{
			this.Versions.Clear();

			foreach (var featureVersion in versionsProvider(context))
			{
				this.Versions.Add(new FeatureVersionViewModel(featureVersion));
			}

			// Set the field to avoid reloading exceptions
			_version = this.Versions.FirstOrDefault();
		}

		private void LoadExceptions(ITransactionContext context, Func<ExceptionsDataLoadParams, IEnumerable<FeatureException>> exceptionsProvider)
		{
			this.Exceptions.Clear();

			foreach (var featureException in exceptionsProvider(this.ExceptionsDataLoadParams(context)))
			{
				this.Exceptions.Add(new FeatureExceptionViewModel(featureException));
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

	public sealed class FeatureException
	{
		public long Id { get; }
		public string Contents { get; }
		public DateTime CreatedAt { get; }
		public FeatureUser User { get; }
		public FeatureVersion Version { get; }

		public FeatureException(long id, string contents, DateTime createdAt, FeatureUser user, FeatureVersion version)
		{
			if (contents == null) throw new ArgumentNullException(nameof(contents));
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Id = id;
			this.Contents = contents;
			this.CreatedAt = createdAt;
			this.User = user;
			this.Version = version;
		}
	}

	public sealed class FeatureExceptionViewModel
	{
		public FeatureException Exception { get; }

		public string User { get; }
		public string Version { get; }
		public string Contents { get; }
		public DateTime CreatedAt { get; }

		public FeatureExceptionViewModel(FeatureException exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Exception = exception;

			this.User = exception.User.Name;
			this.Version = exception.Version.Name;
			this.Contents = exception.Contents;
			this.CreatedAt = exception.CreatedAt;
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

	public sealed class FeatureVersion
	{
		public long Id { get; }
		public string Name { get; }

		public FeatureVersion(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class FeatureVersionViewModel
	{
		public FeatureVersion Version { get; }

		public string Name { get; }

		public FeatureVersionViewModel(FeatureVersion version)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Version = version;
			this.Name = version.Name;
		}
	}
}