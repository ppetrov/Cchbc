using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;
using Cchbc.Features.Db.Objects;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class Setting
	{
		public string Name { get; }
		public string Value { get; }

		public Setting(string name, string value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (value == null) throw new ArgumentNullException(nameof(value));

			this.Name = name;
			this.Value = value;
		}
	}

	public static class SettingHelper
	{
		public static int ParseInt(string input)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			var value = input.Trim();
			if (value != string.Empty)
			{
				int number;
				if (int.TryParse(value, out number))
				{
					return number;
				}

				// TODO : Log warning - cannot parse int value
			}

			return 0;
		}
	}

	public sealed class DashboardSettings
	{
		public int NumberOfUsersToDisplay { get; set; } = 10;
	}

	public sealed class DashboardSettingsManager
	{
		public DashboardSettingsAdapter Adapter { get; }

		public DashboardSettingsManager(DashboardSettingsAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public void Load(ITransactionContext context, DashboardSettings settings)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			foreach (var setting in this.Adapter.GetSettings(context))
			{
				if (setting.Name.Equals(nameof(DashboardSettings.NumberOfUsersToDisplay), StringComparison.OrdinalIgnoreCase))
				{
					settings.NumberOfUsersToDisplay = SettingHelper.ParseInt(setting.Value);
					break;
				}
			}
		}
	}

	public sealed class DashboardSettingsAdapter
	{
		public List<Setting> GetSettings(ITransactionContext context)
		{
			var settings = new List<Setting>();

			// TODO : !!! Load settings from database

			return settings;
		}
	}

	public sealed class DashboardViewModel : ViewModel
	{
		private Dashboard Dashboard { get; }

		public string FindUserCaption { get; }
		public string UsersCaption { get; }
		public ObservableCollection<DashboardUserViewModel> Users => this.Dashboard.Users;

		public string UsageCaption { get; }
		public string ExceptionsCaption { get; }

		public ICommand SearchUsersCommand { get; }

		public DashboardViewModel(Dashboard dashboard)
		{
			if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));

			this.Dashboard = dashboard;

			this.UsersCaption = @"Users";
			this.UsageCaption = @"Usage";
			this.ExceptionsCaption = @"Exceptions";
			this.FindUserCaption = @"Find";

			this.SearchUsersCommand = new RelayCommand(() =>
			{
				// Display content dialog ???
			});
		}



		public void Load()
		{
			this.Dashboard.Load();
		}
	}

	public sealed class Dashboard
	{
		private CommonDataProvider DataProvider { get; } = new CommonDataProvider();
		private ITransactionContextCreator ContextCreator { get; }
		private DashboardSettings Settings { get; } = new DashboardSettings();
		private DashboardSettingsManager SettingsManager { get; } = new DashboardSettingsManager(new DashboardSettingsAdapter());
		private DashboardVersionManager VersionManager { get; } = new DashboardVersionManager(new DashboardVersionAdapter());

		public ObservableCollection<DashboardUserViewModel> Users { get; } = new ObservableCollection<DashboardUserViewModel>();
		public ObservableCollection<DashboardVersion> Versions { get; } = new ObservableCollection<DashboardVersion>();

		public Dashboard(ITransactionContextCreator contextCreator)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			this.ContextCreator = contextCreator;
		}

		public void Load()
		{
			using (var ctx = this.ContextCreator.Create())
			{
				this.DataProvider.Load(ctx);

				this.LoadSettings(ctx);
				this.LoadUsers();
				this.LoadVersions(ctx);

				ctx.Complete();
			}
		}

		private void LoadSettings(ITransactionContext context)
		{
			this.SettingsManager.Load(context, this.Settings);
		}

		private void LoadUsers()
		{
			var versions = this.DataProvider.Versions;

			this.Users.Clear();
			foreach (var user in this.DataProvider.Users.Values)
			{
				this.Users.Add(new DashboardUserViewModel(new DashboardUser(user.Id, user.Name, versions[user.VersionId].Name, user.ReplicatedAt)));
			}
		}

		private void LoadVersions(ITransactionContext context)
		{
			var versions = this.VersionManager.GetBy(this.DataProvider, context);

			this.Versions.Clear();
			foreach (var version in versions)
			{
				this.Versions.Add(version);
			}
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
		public int ExceptionsCount { get; }

		public DashboardVersion(DbFeatureVersionRow version, int exceptionsCount)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Version = version;
			this.ExceptionsCount = exceptionsCount;
		}
	}

	public sealed class DashboardVersionManager
	{
		public DashboardVersionAdapter Adapter { get; }

		public DashboardVersionManager(DashboardVersionAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public List<DashboardVersion> GetBy(CommonDataProvider dataProvider, ITransactionContext context)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (context == null) throw new ArgumentNullException(nameof(context));

			var versions = dataProvider.Versions;
			var result = this.Adapter.GetBy(context, versions);

			var dashboardVersions = new List<DashboardVersion>(result.Count);

			foreach (var pair in result)
			{
				dashboardVersions.Add(new DashboardVersion(versions[pair.Key], pair.Value));
			}

			return dashboardVersions;
		}
	}

	public sealed class DashboardVersionAdapter
	{
		public Dictionary<long, int> GetBy(ITransactionContext context, Dictionary<long, DbFeatureVersionRow> versions)
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