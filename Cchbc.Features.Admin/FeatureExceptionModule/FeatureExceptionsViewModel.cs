using System;
using System.Collections.ObjectModel;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureExceptionModule.Managers;
using Cchbc.Features.Admin.FeatureExceptionModule.ViewModels;
using Cchbc.Features.Admin.Helpers;
using Cchbc.Features.Admin.Providers;
using Cchbc.Features.Admin.ViewModels;
using Cchbc.Objects;


namespace Cchbc.Features.Admin.FeatureExceptionModule
{
    public sealed class FeatureExceptionsViewModel : ViewModel
    {
        private ITransactionContextCreator ContextCreator { get; }
        private CommonDataProvider DataProvider { get; }
        private IFeatureExceptionManager FeatureExceptionManager { get; }

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
        public ObservableCollection<FeatureExceptionViewModel> Exceptions { get; } = new ObservableCollection<FeatureExceptionViewModel>();

        public FeatureExceptionsViewModel(ITransactionContextCreator contextCreator, CommonDataProvider dataProvider, IFeatureExceptionManager featureExceptionManager)
        {
            if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));
            if (featureExceptionManager == null) throw new ArgumentNullException(nameof(featureExceptionManager));

            this.ContextCreator = contextCreator;
            this.DataProvider = dataProvider;
            this.FeatureExceptionManager = featureExceptionManager;

            foreach (var timePeriodViewModel in TimePeriodHelper.GetStandardPeriods())
            {
                this.TimePeriods.Add(timePeriodViewModel);
            }

            // Use the field to avoid calling the method LoadData 
            _currentTimePeriod = this.TimePeriods[0];
        }

        public void LoadDataForCurrentTimePeriod()
        {
            using (var ctx = this.ContextCreator.Create())
            {
                this.Exceptions.Clear();

                foreach (var featureException in this.FeatureExceptionManager.GetBy(this.DataProvider, ctx, this.CurrentTimePeriod.TimePeriod))
                {
                    this.Exceptions.Add(new FeatureExceptionViewModel(featureException));
                }

                ctx.Complete();
            }
        }
    }
}