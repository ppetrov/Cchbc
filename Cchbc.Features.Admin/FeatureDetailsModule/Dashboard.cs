using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;
using Cchbc.Features.Db.Objects;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public static class DashboardDataProvider
	{
		public static Task<List<DashboardUser>> GetUsersAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var commonDataProvider = loadParams.DataProvider;

			var sqlParams = new[]
			{
				new QueryParameter(@"@MAXUSERS", loadParams.Settings.MaxUsersToLoad),
			};
			var query = new Query<DbFeatureUserRow>(@"SELECT ID, NAME, REPLICATED_AT, VERSION_ID FROM FEATURE_USERS ORDER BY REPLICATED_AT DESC LIMIT @MAXUSERS", FeatureUserRowCreator, sqlParams);

			var users = new List<DashboardUser>();
			var versions = commonDataProvider.Versions;

			foreach (var userRow in loadParams.Context.Execute(query))
			{
				users.Add(new DashboardUser(userRow.Id, userRow.Name, versions[userRow.VersionId].Name, userRow.ReplicatedAt));
			}

			return Task.FromResult(users);
		}

		private static DbFeatureUserRow FeatureUserRowCreator(IFieldDataReader r)
		{
			return new DbFeatureUserRow(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), r.GetInt64(3));
		}

		public static Task<List<DashboardVersion>> GetVersionsAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var context = loadParams.Context;
			var versions = loadParams.DataProvider.Versions;
			var dashboardVersions = new List<DashboardVersion>(versions.Count);

			var exceptions = CountExceptionsByVersion(context, versions);
			var users = CountUsersByVersion(context, versions);

			foreach (var versionPair in versions)
			{
				var versionId = versionPair.Key;
				var version = versionPair.Value;

				int usersCount;
				users.TryGetValue(versionId, out usersCount);

				int exceptionsCount;
				exceptions.TryGetValue(versionId, out exceptionsCount);

				dashboardVersions.Add(new DashboardVersion(version, usersCount, exceptionsCount));
			}

			dashboardVersions.Sort((x, y) =>
			{
				var cmp = string.Compare(x.Version.Name, y.Version.Name, StringComparison.OrdinalIgnoreCase);
				return cmp;
			});

			return Task.FromResult(dashboardVersions);
		}

		public static Task<List<DashboardException>> GetExceptionsAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var settings = loadParams.Settings;
			var relativeTimePeriod = settings.ExceptionsRelativeTimePeriod;
			var samples = settings.ExceptionsChartSamples;

			var rangeTimePeriod = relativeTimePeriod.ToRange(DateTime.Now);

			var query = new Query(@"SELECT CREATED_AT FROM FEATURE_EXCEPTIONS WHERE @FROMDATE <= CREATED_AT AND CREATED_AT <= @TODATE", new[]
			{
				new QueryParameter(@"@FROMDATE", rangeTimePeriod.FromDate),
				new QueryParameter(@"@TODATE", rangeTimePeriod.ToDate),
			});

			var step = ((long)relativeTimePeriod.TimeOffset.TotalMilliseconds) / samples;
			var baseDate = rangeTimePeriod.FromDate;

			var map = new int[samples];

			loadParams.Context.Fill(new Dictionary<DateTime, int>(0), (r, m) =>
			{
				var delta = ((long)(r.GetDateTime(0) - baseDate).TotalMilliseconds) / step;
				map[delta]++;
			}, query);

			var result = new List<DashboardException>(samples);

			for (var i = 0; i < map.Length; i++)
			{
				result.Add(new DashboardException(baseDate.AddMilliseconds(step * i), map[i]));
			}

			return Task.FromResult(result);
		}

		public static Task<List<DashboardFeatureByCount>> GetMostUsedFeaturesAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var mostUsedFeatures = new List<DashboardFeatureByCount>();

			mostUsedFeatures.Add(new DashboardFeatureByCount(1, @"Copy Activity", 123));
			mostUsedFeatures.Add(new DashboardFeatureByCount(2, @"Close Activity", 77));

			return Task.FromResult(mostUsedFeatures);
		}

		private static Dictionary<long, int> CountExceptionsByVersion(ITransactionContext context, Dictionary<long, DbFeatureVersionRow> versions)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (versions == null) throw new ArgumentNullException(nameof(versions));

			var map = new Dictionary<long, int>(versions.Count);

			foreach (var row in versions)
			{
				map.Add(row.Key, 0);
			}

			context.Fill(map, (r, m) =>
			{
				var versionId = r.GetInt64(0);
				var count = r.GetInt32(1);

				m[versionId] = count;

			}, new Query(@"SELECT VERSION_ID, COUNT(*) FROM FEATURE_EXCEPTIONS GROUP BY VERSION_ID"));

			return map;
		}

		private static Dictionary<long, int> CountUsersByVersion(ITransactionContext context, Dictionary<long, DbFeatureVersionRow> versions)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (versions == null) throw new ArgumentNullException(nameof(versions));

			var map = new Dictionary<long, int>(versions.Count);

			foreach (var row in versions)
			{
				map.Add(row.Key, 0);
			}

			context.Fill(map, (r, m) =>
			{
				var versionId = r.GetInt64(0);
				var count = r.GetInt32(1);

				m[versionId] = count;

			}, new Query(@"SELECT VERSION_ID, COUNT(*) FROM FEATURE_USERS GROUP BY VERSION_ID"));

			return map;
		}


	}

	public sealed class DashboardSettings
	{
		public int MaxUsersToLoad { get; } = 5;

		public int VersionsChartSamples { get; } = 10;

		// Exceptions
		public int ExceptionsChartSamples { get; } = 30;
		public RelativeTimePeriod ExceptionsRelativeTimePeriod { get; } = new RelativeTimePeriod(TimeSpan.FromDays(2), RelativeTimeType.Past);
	}

	public sealed class DashboardViewModel : ViewModel
	{
		private Dashboard Dashboard { get; }

		public ObservableCollection<DashboardUserViewModel> Users => this.Dashboard.Users;
		public ObservableCollection<DashboardVersionViewModel> Versions => this.Dashboard.Versions;
		public ObservableCollection<DashboardExceptionViewModel> Exceptions => this.Dashboard.Exceptions;
		public ObservableCollection<DashboardFeatureByCountViewModel> MostUsedFeatures => this.Dashboard.MostUsedFeatures;
		public ObservableCollection<DashboardFeatureByTimeViewModel> SlowestFeatures => this.Dashboard.SlowestFeatures;

		public string UsersCaption { get; }
		public string StatsCaption { get; }
		public string VersionStatsReportCaption { get; }
		public string ExceptionsCaptions { get; }
		public string UsageCaption { get; }

		private bool _isUsersLoading;
		public bool IsUsersLoading
		{
			get { return _isUsersLoading; }
			set { this.SetField(ref _isUsersLoading, value); }
		}

		public int TotalUsers { get; } = 123;
		public int TotalVersions { get; } = 7;
		public int TotalExceptions { get; } = 138457;
		public int TotalFeatures { get; } = 35;

		public DashboardViewModel(Dashboard dashboard)
		{
			if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));

			this.Dashboard = dashboard;

			// TODO : Load captions !!!!
			this.UsersCaption = @"Users";
			this.StatsCaption = @"Statistics";
			this.VersionStatsReportCaption = @"By Version statistics";
			this.UsageCaption = @"Most used/Slowest features";
			this.ExceptionsCaptions = @"Exceptions for the last 24 hours";
		}

		public Task LoadAsync(ITransactionContext context,
			Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider,
			Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider,
			Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> mostUsedFeaturesProvider)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (mostUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(mostUsedFeaturesProvider));

			Func<DashboarLoadParams, Task<List<DashboardUser>>> withProgressUsersProvider = arg =>
			{
				this.IsUsersLoading = true;
				return usersProvider(arg).ContinueWith(t =>
				{
					this.IsUsersLoading = false;
					return t.Result;
				}, TaskScheduler.FromCurrentSynchronizationContext());
			};

			return this.Dashboard.LoadAsync(context, withProgressUsersProvider, versionsProvider, exceptionsProvider, mostUsedFeaturesProvider);
		}
	}

	public sealed class Dashboard
	{
		public ObservableCollection<DashboardUserViewModel> Users { get; } = new ObservableCollection<DashboardUserViewModel>();
		public ObservableCollection<DashboardVersionViewModel> Versions { get; } = new ObservableCollection<DashboardVersionViewModel>();
		public ObservableCollection<DashboardExceptionViewModel> Exceptions { get; } = new ObservableCollection<DashboardExceptionViewModel>();
		public ObservableCollection<DashboardFeatureByCountViewModel> MostUsedFeatures { get; } = new ObservableCollection<DashboardFeatureByCountViewModel>();
		public ObservableCollection<DashboardFeatureByCountViewModel> LeastUsedFeatures { get; } = new ObservableCollection<DashboardFeatureByCountViewModel>();
		public ObservableCollection<DashboardFeatureByTimeViewModel> SlowestFeatures { get; } = new ObservableCollection<DashboardFeatureByTimeViewModel>();

		public async Task LoadAsync(ITransactionContext context,
			Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider,
			Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider,
			Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> mostUsedFeaturesProvider)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));

			// Load common data - users, version, settings, features
			var dataProvider = new CommonDataProvider();
			dataProvider.Load(context);

			var settings = GetDashboardSettings(dataProvider);
			var loadParams = new DashboarLoadParams(context, settings, dataProvider);

			var tasks = new[]
			{
				this.LoadUsersAsync(loadParams, usersProvider),
				this.LoadVersionsAsync(loadParams, versionsProvider),
				this.LoadExceptionsAsync(loadParams, exceptionsProvider),
				this.LoadMostUsedFeaturesAsync(loadParams, mostUsedFeaturesProvider),
				//this.LoadLeastUsedFeaturesAsync(loadParams, null),
				//this.LoadSlowestUsedFeaturesAsync(loadParams, null),
			};

			await Task.WhenAll(tasks);
		}

		private static DashboardSettings GetDashboardSettings(CommonDataProvider dataProvider)
		{
			List<Setting> values;
			dataProvider.Settings.TryGetValue(nameof(Dashboard), out values);

			var settings = new DashboardSettings();
			foreach (var setting in values ?? new List<Setting>(0))
			{
				//TODO : !!!
				//if (setting.Name.Equals(nameof(DashboardSettings.NumberOfUsersToDisplay), StringComparison.OrdinalIgnoreCase))
				//{
				//	settings.NumberOfUsersToDisplay = ValueParser.ParseInt(setting.Value) ?? 10;
				//	break;
				//}
			}
			return settings;
		}

		private async Task LoadUsersAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider)
		{
			var users = await usersProvider(loadParams);

			this.Users.Clear();
			foreach (var user in users)
			{
				this.Users.Add(new DashboardUserViewModel(user));
			}
		}

		private async Task LoadVersionsAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider)
		{
			var versions = await versionsProvider(loadParams);

			this.Versions.Clear();
			foreach (var version in versions)
			{
				this.Versions.Add(new DashboardVersionViewModel(version));
			}
		}

		private async Task LoadExceptionsAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider)
		{
			var exceptions = await exceptionsProvider(loadParams);

			this.Exceptions.Clear();
			foreach (var exception in exceptions)
			{
				this.Exceptions.Add(new DashboardExceptionViewModel(exception));
			}
		}

		private async Task LoadMostUsedFeaturesAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> featuresProvider)
		{
			//var mostUsedLimit = 10;
			//var mostUsed = this.FeatureManager.GetMostUsed(context, mostUsedLimit);
			//this.MostUsedFeatures.Clear();
			//foreach (var byCount in mostUsed)
			//{
			//	this.MostUsedFeatures.Add(new DashboardFeatureByCountViewModel(byCount));
			//}
		}

		private async Task LoadLeastUsedFeaturesAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> featuresProvider)
		{
			//var leastUsedLimit = 10;
			//var leastUsed = this.FeatureManager.GetLeastUsed(context, leastUsedLimit);
			//this.LeastUsedFeatures.Clear();
			//foreach (var byCount in leastUsed)
			//{
			//	this.LeastUsedFeatures.Add(new DashboardFeatureByCountViewModel(byCount));
			//}
		}

		private async Task LoadSlowestUsedFeaturesAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> featuresProvider)
		{
			//var slowestLimit = 10;
			//var slowest = this.FeatureManager.GetSlowest(context, slowestLimit);
			//this.SlowestFeatures.Clear();
			//foreach (var byTime in slowest)
			//{
			//	this.SlowestFeatures.Add(new DashboardFeatureByTimeViewModel(byTime));
			//}
		}
	}

	public sealed class DashboarLoadParams
	{
		public ITransactionContext Context { get; }
		public DashboardSettings Settings { get; }
		public CommonDataProvider DataProvider { get; }

		public DashboarLoadParams(ITransactionContext context, DashboardSettings settings, CommonDataProvider dataProvider)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.Context = context;
			this.Settings = settings;
			this.DataProvider = dataProvider;
		}
	}

	public sealed class DashboardVersionViewModel : ViewModel
	{
		public DashboardVersion Version { get; }

		public string Name { get; }
		public int Users { get; }
		public int Exceptions { get; }

		public DashboardVersionViewModel(DashboardVersion version)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Version = version;
			this.Name = version.Version.Name;
			this.Users = version.Users;
			this.Exceptions = version.Exceptions;
		}
	}

	public sealed class DashboardException
	{
		public DateTime DateTime { get; }
		public int Count { get; }

		public DashboardException(DateTime dateTime, int count)
		{
			this.DateTime = dateTime;
			this.Count = count;
		}
	}

	public sealed class DashboardFeatureByTime
	{
		public long Id { get; }
		public string Name { get; }
		public TimeSpan TimeSpent { get; }

		public DashboardFeatureByTime(long id, string name, TimeSpan timeSpent)
		{
			this.Id = id;
			this.Name = name;
			this.TimeSpent = timeSpent;
		}
	}

	public sealed class DashboardFeatureByCount
	{
		public long Id { get; }
		public string Name { get; }
		public int Count { get; }

		public DashboardFeatureByCount(long id, string name, int count)
		{
			this.Id = id;
			this.Name = name;
			this.Count = count;
		}
	}

	public sealed class DashboardFeatureByTimeViewModel : ViewModel
	{
		public DashboardFeatureByTime Feature { get; }

		public string Name { get; }
		public TimeSpan TimeSpent { get; }

		public DashboardFeatureByTimeViewModel(DashboardFeatureByTime feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Feature = feature;
			this.Name = feature.Name;
			this.TimeSpent = feature.TimeSpent;
		}
	}

	public sealed class DashboardFeatureByCountViewModel : ViewModel
	{
		public DashboardFeatureByCount Feature { get; }

		public string Name { get; }
		public int Count { get; }

		public DashboardFeatureByCountViewModel(DashboardFeatureByCount feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Feature = feature;
			this.Name = feature.Name;
			this.Count = feature.Count;
		}
	}

	public sealed class DashboardExceptionViewModel : ViewModel
	{
		private DashboardException Exception { get; }

		public DateTime DateTime { get; }
		public int Count { get; }

		public DashboardExceptionViewModel(DashboardException exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Exception = exception;
			this.DateTime = exception.DateTime;
			this.Count = exception.Count;
		}
	}

	public sealed class DashboardUserViewModel : ViewModel
	{
		public DashboardUser User { get; }

		public string Name { get; }
		public string Version { get; }
		public string ReplicatedAt { get; }

		public DashboardUserViewModel(DashboardUser user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.Name = user.Name;
			this.Version = user.Version;
			this.ReplicatedAt = user.ReplicatedAt.ToString(@"T");
		}
	}


	public sealed class DashboardUser
	{
		public long Id { get; }
		public string Name { get; }
		public string Version { get; }
		public DateTime ReplicatedAt { get; }

		public DashboardUser(long id, string name, string version, DateTime replicatedAt)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Id = id;
			this.Name = name;
			this.Version = version;
			this.ReplicatedAt = replicatedAt;
		}
	}

	public sealed class DashboardVersion
	{
		public DbFeatureVersionRow Version { get; }
		public int Users { get; }
		public int Exceptions { get; }

		public DashboardVersion(DbFeatureVersionRow version, int users, int exceptions)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Version = version;
			this.Users = users;
			this.Exceptions = exceptions;
		}
	}





	public sealed class DashboardFeatureManager
	{
		private DashboardFeatureAdapter Adapter { get; }

		public DashboardFeatureManager(DashboardFeatureAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public List<DashboardFeatureByTime> GetSlowest(ITransactionContext context, int limit)
		{
			return this.Adapter.GetSlowest(context, limit);
		}

		public List<DashboardFeatureByCount> GetMostUsed(ITransactionContext context, int limit)
		{
			return this.Adapter.GetMostUsed(context, limit);
		}

		public List<DashboardFeatureByCount> GetLeastUsed(ITransactionContext context, int limit)
		{
			return this.Adapter.GetLeastUsed(context, limit);
		}
	}

	public sealed class DashboardFeatureAdapter
	{
		public List<DashboardFeatureByTime> GetSlowest(ITransactionContext context, int limit)
		{
			// TODO : Add ability to exclude some features
			// Most used will be LoadAgenda & Sync Data
			// Slowest will be Sync Data

			// Select * from features ... where ... and not exist (select 1 from excluded_features where type = 'Time')

			return new List<DashboardFeatureByTime>(limit);
		}

		public List<DashboardFeatureByCount> GetMostUsed(ITransactionContext context, int limit)
		{
			// TODO : Add ability to exclude some features
			// Most used will be LoadAgenda & Sync Data
			// Slowest will be Sync Data

			// Select * from features ... where ... and not exist (select 1 from excluded_features where type = 'CountMax')

			return new List<DashboardFeatureByCount>(limit);
		}

		public List<DashboardFeatureByCount> GetLeastUsed(ITransactionContext context, int limit)
		{
			// TODO : Add ability to exclude some features
			// Most used will be LoadAgenda & Sync Data
			// Slowest will be Sync Data

			// Select * from features ... where ... and not exist (select 1 from excluded_features where type = 'CountMin')

			return new List<DashboardFeatureByCount>(limit);
		}
	}




	public sealed class FeaturesHeader : ViewModel
	{
		public string Caption { get; }

		private FeatureSortOrder _sortOrder;

		public FeatureSortOrder SortOrder
		{
			get { return _sortOrder; }
			set { this.SetField(ref _sortOrder, value); }
		}

		public ICommand ChangeSortOrderCommand { get; }

		public FeaturesHeader(string caption, FeatureSortOrder sortOrder, ICommand changeSortOrderCommand)
		{
			if (caption == null) throw new ArgumentNullException(nameof(caption));
			if (sortOrder == null) throw new ArgumentNullException(nameof(sortOrder));

			this.Caption = caption;
			this._sortOrder = sortOrder;
			this.ChangeSortOrderCommand = changeSortOrderCommand;
		}
	}

	public sealed class FeatureSortOrder : ViewModel
	{
		public static readonly FeatureSortOrder Alphabetical = new FeatureSortOrder(@"Alphabetical");
		public static readonly FeatureSortOrder MostUsed = new FeatureSortOrder(@"Most Used");

		public string Name { get; }

		public FeatureSortOrder(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}

	public sealed class FeatureDetailsViewModel : ViewModel
	{
		private CommonDataProvider DataProvider { get; }
		private ITransactionContextCreator ContextCreator { get; }

		public string ContextsHeader { get; } = @"Screens";
		public FeaturesHeader FeaturesHeader { get; }


		public ObservableCollection<ContextViewModel> Contexts { get; } = new ObservableCollection<ContextViewModel>();
		public ObservableCollection<FeatureViewModel> Features { get; } = new ObservableCollection<FeatureViewModel>();

		private ContextViewModel _currentContext;

		public ContextViewModel CurrentContext
		{
			get { return _currentContext; }
			set
			{
				_currentContext = value;
				this.LoadCurrentFeatures();
			}
		}

		private FeatureViewModel _currentFeature;

		public FeatureViewModel CurrentFeature
		{
			get { return _currentFeature; }
			set
			{
				_currentFeature = value;
				this.LoadCurrentFeatures();
			}
		}

		public FeatureDetailsViewModel(CommonDataProvider dataProvider, ITransactionContextCreator contextCreator)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			this.DataProvider = dataProvider;
			this.ContextCreator = contextCreator;

			this.FeaturesHeader = new FeaturesHeader(@"Features", FeatureSortOrder.Alphabetical, new RelayCommand(() =>
			{
				if (this.FeaturesHeader.SortOrder == FeatureSortOrder.Alphabetical)
				{
					this.FeaturesHeader.SortOrder = FeatureSortOrder.MostUsed;
				}
				else
				{
					if (this.FeaturesHeader.SortOrder == FeatureSortOrder.MostUsed)
					{
						this.FeaturesHeader.SortOrder = FeatureSortOrder.Alphabetical;
					}
				}

				this.LoadCurrentFeatures();
			}));

			this.Contexts.Add(new ContextViewModel(new DbContextRow(-1, @"All")));
			foreach (var context in dataProvider.Contexts.Values)
			{
				this.Contexts.Add(new ContextViewModel(context));
			}

			this.CurrentContext = this.Contexts[0];
		}

		private void LoadCurrentFeatures()
		{
			var contextId = this.CurrentContext.Context.Id;

			var relatedFeatures = new List<FeatureViewModel>();
			foreach (var feature in this.DataProvider.Features.Values)
			{
				if (feature.ContextId == contextId)
				{
					relatedFeatures.Add(new FeatureViewModel(feature));
				}
			}

			//_currentFeature

			if (this.FeaturesHeader.SortOrder == FeatureSortOrder.Alphabetical)
			{
				relatedFeatures.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			}
			if (this.FeaturesHeader.SortOrder == FeatureSortOrder.MostUsed)
			{
				relatedFeatures.Sort((x, y) =>
				{
					var cmp = x.Count.CompareTo(y.Count);
					if (cmp == 0)
					{
						cmp = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
					}
					return cmp;
				});
			}

			this.Features.Clear();
			foreach (var viewModel in relatedFeatures)
			{
				this.Features.Add(viewModel);
			}
		}
	}

	public sealed class ContextViewModel : ViewModel
	{
		public DbContextRow Context { get; }

		public string Name { get; }

		public ContextViewModel(DbContextRow context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			this.Name = context.Name;
		}
	}

	public sealed class FeatureViewModel : ViewModel
	{
		public DbFeatureRow Feature { get; }

		public string Name { get; }

		private int _count;

		public int Count
		{
			get { return _count; }
			set { this.SetField(ref _count, _count); }
		}

		public FeatureViewModel(DbFeatureRow feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Feature = feature;
			this.Name = feature.Name;
		}
	}
}