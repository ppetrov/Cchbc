using System;
using System.Collections.ObjectModel;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUsageModule.Adapters;
using Cchbc.Features.Admin.FeatureUsageModule.Managers;
using Cchbc.Features.Admin.FeatureUsageModule.Objects;
using Cchbc.Features.Admin.FeatureUsageModule.ViewModels;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureUsageModule
{
    public sealed class FeatureUsagesViewModel : ViewModel
    {
        private FeatureUsageManager FeatureUsageManager { get; } = new FeatureUsageManager(new FeatureUsageAdapter());
        private ITransactionContextCreator ContextCreator { get; }

        private TimePeriodViewModel _currentTimePeriod;
        public TimePeriodViewModel CurrentTimePeriod
        {
            get { return _currentTimePeriod; }
            set
            {
                this.SetField(ref _currentTimePeriod, value);

                this.LoadDataForCurrentTimePeriod();
            }
        }

        public ObservableCollection<TimePeriodViewModel> TimePeriods { get; } = new ObservableCollection<TimePeriodViewModel>();
        public ObservableCollection<FeatureUsageViewModel> FeatureUsages { get; } = new ObservableCollection<FeatureUsageViewModel>();

        public FeatureUsagesViewModel(ITransactionContextCreator contextCreator)
        {
            if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

            this.ContextCreator = contextCreator;

            var today = DateTime.Today;
            var toDate = today.AddDays(1);
            this.TimePeriods.Add(new TimePeriodViewModel(@"Today", new TimePeriod(toDate.AddDays(-2), toDate)));
            this.TimePeriods.Add(new TimePeriodViewModel(@"Last 7 days", new TimePeriod(toDate.AddDays(-(2 + 7)), toDate)));
            this.TimePeriods.Add(new TimePeriodViewModel(@"Last 30 days", new TimePeriod(toDate.AddDays(-(2 + 30)), toDate)));

            _currentTimePeriod = this.TimePeriods[0];
        }

        public void LoadDataForCurrentTimePeriod()
        {
            using (var context = ContextCreator.Create())
            {
                this.FeatureUsages.Clear();

                foreach (var featureUsage in this.FeatureUsageManager.GetBy(context, this.CurrentTimePeriod.TimePeriod))
                {
                    this.FeatureUsages.Add(new FeatureUsageViewModel(featureUsage));
                }

                context.Complete();
            }
        }
    }
}
