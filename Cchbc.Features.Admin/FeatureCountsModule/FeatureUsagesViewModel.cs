﻿using System;
using System.Collections.ObjectModel;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureCountsModule.Adapters;
using Cchbc.Features.Admin.FeatureCountsModule.Managers;
using Cchbc.Features.Admin.FeatureCountsModule.ViewModels;
using Cchbc.Features.Admin.Helpers;
using Cchbc.Features.Admin.Providers;
using Cchbc.Features.Admin.ViewModels;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureCountsModule
{
	public sealed class FeatureUsagesViewModel : ViewModel
	{
		private FeatureCountManager FeatureCountManager { get; } = new FeatureCountManager(new FeatureCountAdapter());

		private ITransactionContextCreator ContextCreator { get; }
		private CommonDataProvider DataProvider { get; }

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

		public FeatureUsagesViewModel(ITransactionContextCreator contextCreator, CommonDataProvider dataProvider)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			this.ContextCreator = contextCreator;
			this.DataProvider = dataProvider;

			foreach (var timePeriodViewModel in TimePeriodHelper.GetStandardPeriods())
			{
				this.TimePeriods.Add(timePeriodViewModel);
			}

			// Use the field to avoid calling the method LoadData 
			_currentTimePeriod = this.TimePeriods[0];
		}

		public void LoadDataForCurrentTimePeriod()
		{
			using (var context = ContextCreator.Create())
			{
				this.FeatureUsages.Clear();

				foreach (var featureUsage in this.FeatureCountManager.GetBy(this.DataProvider, context, this.CurrentTimePeriod.TimePeriod))
				{
					this.FeatureUsages.Add(new FeatureUsageViewModel(featureUsage));
				}

				context.Complete();
			}
		}
	}
}