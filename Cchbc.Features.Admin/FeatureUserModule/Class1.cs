using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cchbc.Data;
using Cchbc.Features.Admin.Helpers;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;
using Cchbc.Features.Admin.ViewModels;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureUserModule
{
    public sealed class UserOverviewViewModel : ViewModel
    {
        public string Name { get; }
        public ObservableCollection<FeatureCountByPeriodViewModel> Features { get; }
        public ObservableCollection<FeatureCountByPeriodViewModel> Exceptions { get; }

        public UserOverviewViewModel(UserOverview userOverview, ObservableCollection<TimePeriodViewModel> timePeriods)
        {
            if (userOverview == null) throw new ArgumentNullException(nameof(userOverview));
            if (timePeriods == null) throw new ArgumentNullException(nameof(timePeriods));

            this.Name = userOverview.Name;

            this.Features = new ObservableCollection<FeatureCountByPeriodViewModel>();
            this.Exceptions = new ObservableCollection<FeatureCountByPeriodViewModel>();

            foreach (var byPeriod in userOverview.Features)
            {
                this.Features.Add(new FeatureCountByPeriodViewModel(new TimePeriodViewModel("TODO : !!!", byPeriod.TimePeriod), byPeriod.Count));
            }

            foreach (var byPeriod in userOverview.Exceptions)
            {

            }
        }
    }

    public sealed class UsersOverviewViewModel : ViewModel
    {
        private UserOverviewManager Manager { get; } = new UserOverviewManager(new UserFeatureCountAdapter());
        private CommonDataProvider DataProvider { get; }

        public ObservableCollection<TimePeriodViewModel> TimePeriods { get; } = new ObservableCollection<TimePeriodViewModel>();
        public ObservableCollection<UserOverviewViewModel> Users { get; } = new ObservableCollection<UserOverviewViewModel>();

        public UsersOverviewViewModel(CommonDataProvider dataProvider)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

            this.DataProvider = dataProvider;

            foreach (var viewModel in TimePeriodHelper.GetStandardPeriods())
            {
                this.TimePeriods.Add(viewModel);
            }
        }

        public void Load(ITransactionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var periods = new TimePeriod[this.TimePeriods.Count];
            for (var i = 0; i < this.TimePeriods.Count; i++)
            {
                periods[i] = this.TimePeriods[i].TimePeriod;
            }

            var data = this.Manager.GetBy(this.DataProvider, context, periods);
            var index = 0;
            var viewModels = new UserOverviewViewModel[data.Count];
            foreach (var userOverview in data.Values)
            {
                viewModels[index++] = new UserOverviewViewModel(userOverview, this.TimePeriods);
            }

            this.Users.Clear();
            foreach (var viewModel in viewModels)
            {
                this.Users.Add(viewModel);
            }
        }
    }

    public sealed class UserOverview
    {
        public string Name { get; }
        public List<FeatureCountByPeriod> Features { get; }
        public List<FeatureCountByPeriod> Exceptions { get; }

        public UserOverview(string name, TimePeriod[] timePeriods)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (timePeriods == null) throw new ArgumentNullException(nameof(timePeriods));

            this.Name = name;
            this.Features = new List<FeatureCountByPeriod>(timePeriods.Length);
            this.Exceptions = new List<FeatureCountByPeriod>(timePeriods.Length);

            foreach (var timePeriod in timePeriods)
            {
                this.Features.Add(new FeatureCountByPeriod(timePeriod));
                this.Exceptions.Add(new FeatureCountByPeriod(timePeriod));
            }
        }

        public void SetupFeature(TimePeriod timePeriod, UserFeatureCount featureCount)
        {
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));
            if (featureCount == null) throw new ArgumentNullException(nameof(featureCount));

            this.Setup(this.Features, timePeriod, featureCount);
        }

        public void SetupException(TimePeriod timePeriod, UserFeatureCount featureCount)
        {
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));
            if (featureCount == null) throw new ArgumentNullException(nameof(featureCount));

            this.Setup(this.Exceptions, timePeriod, featureCount);
        }

        private void Setup(List<FeatureCountByPeriod> source, TimePeriod timePeriod, UserFeatureCount featureCount)
        {
            foreach (var byPeriod in source)
            {
                if (byPeriod.TimePeriod == timePeriod)
                {
                    byPeriod.Count = featureCount.Value;
                    break;
                }
            }
        }
    }

    public sealed class UserOverviewManager
    {
        public UserFeatureCountAdapter Adapter { get; }

        public UserOverviewManager(UserFeatureCountAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            this.Adapter = adapter;
        }

        public Dictionary<string, UserOverview> GetBy(CommonDataProvider dataProvider, ITransactionContext context, TimePeriod[] timePeriods)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (timePeriods == null) throw new ArgumentNullException(nameof(timePeriods));

            var map = new Dictionary<string, UserOverview>();

            foreach (var timePeriod in timePeriods)
            {
                foreach (var featureCount in this.Adapter.GetFeaturesBy(dataProvider, context, timePeriod))
                {
                    var userOverview = GetOrCreateUserOverview(map, featureCount.User, timePeriods);
                    userOverview.SetupFeature(timePeriod, featureCount);
                }

                foreach (var featureCount in this.Adapter.GetExceptionsBy(dataProvider, context, timePeriod))
                {
                    var userOverview = GetOrCreateUserOverview(map, featureCount.User, timePeriods);
                    userOverview.SetupException(timePeriod, featureCount);
                }
            }

            return map;
        }

        private static UserOverview GetOrCreateUserOverview(Dictionary<string, UserOverview> map, string user, TimePeriod[] timePeriods)
        {
            UserOverview userOverview;
            if (!map.TryGetValue(user, out userOverview))
            {
                userOverview = new UserOverview(user, timePeriods);
                map.Add(user, userOverview);
            }
            return userOverview;
        }
    }


    public sealed class FeatureCountByPeriod
    {
        public TimePeriod TimePeriod { get; }
        public int Count { get; set; }

        public FeatureCountByPeriod(TimePeriod timePeriod)
        {
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            this.TimePeriod = timePeriod;
        }
    }


    public sealed class FeatureCountByPeriodViewModel : ViewModel
    {
        public TimePeriodViewModel TimePeriod { get; }
        public int Count { get; }

        public FeatureCountByPeriodViewModel(TimePeriodViewModel timePeriod, int count)
        {
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            this.TimePeriod = timePeriod;
            this.Count = count;
        }
    }

    public sealed class UserFeatureCount
    {
        public string User { get; }
        public int Value { get; }

        public UserFeatureCount(string user, int value)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            this.User = user;
            this.Value = value;
        }
    }

    public sealed class UserFeatureCountAdapter
    {
        private sealed class UserFeatureCountRow
        {
            public readonly long UserId;
            public readonly int Count;

            public UserFeatureCountRow(long userId, int count)
            {
                this.UserId = userId;
                this.Count = count;
            }
        }

        public UserFeatureCount[] GetFeaturesBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            var query = @"
            SELECT
                      E.USER_ID,
                      COUNT(*)
            FROM
                      FEATURE_ENTRIES E
            WHERE   @FROMDATE        < E.CREATED_AT AND E.CREATED_AT < @TODATE
            GROUP BY
                      E.USER_ID";

            return GetBy(provider, context, timePeriod, query);
        }

        public UserFeatureCount[] GetExceptionsBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            var query = @"
            SELECT
                      E.USER_ID,
                      COUNT(*)
            FROM
                      FEATURE_EXCEPTIONS E
            WHERE   @FROMDATE        < E.CREATED_AT AND E.CREATED_AT < @TODATE
            GROUP BY
                      E.USER_ID";

            return GetBy(provider, context, timePeriod, query);
        }

        private UserFeatureCount[] GetBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod,
            string query)
        {
            var sqlParams = new[]
            {
                new QueryParameter(@"@FROMDATE", timePeriod.FromDate),
                new QueryParameter(@"@TODATE", timePeriod.ToDate),
            };

            var rows = context.Execute(new Query<UserFeatureCountRow>(query, this.UserFeatureCountRowCreator, sqlParams));

            var userFeatureCounts = new UserFeatureCount[rows.Count];

            var users = provider.Users;

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                userFeatureCounts[i] = new UserFeatureCount(users[row.UserId].Name, row.Count);
            }

            return userFeatureCounts;
        }

        private UserFeatureCountRow UserFeatureCountRowCreator(IFieldDataReader r)
        {
            return new UserFeatureCountRow(r.GetInt64(0), r.GetInt32(1));
        }
    }

}
