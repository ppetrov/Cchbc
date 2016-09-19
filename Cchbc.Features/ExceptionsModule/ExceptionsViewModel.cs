using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cchbc.Data;
using Cchbc.Features.ExceptionsModule.Objects;
using Cchbc.Features.ExceptionsModule.Rows;
using Cchbc.Objects;

namespace Cchbc.Features.ExceptionsModule
{
	public sealed class ExceptionsViewModel : ViewModel
	{
		private bool IsLoaded { get; set; }

		private ExceptionsDataLoadParams GetExceptionsDataLoadParams(ITransactionContext context)
		{
			var loadParams = new ExceptionsDataLoadParams(context, this.Settings.MaxExceptionEntries)
			{
				Version = this.Version,
				TimePeriod = this.TimePeriod
			};
			return loadParams;
		}

		public string VersionCaption { get; } = @"Version";
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
				// We need to load the exceptions & counts
				this.LoadCurrentExceptionData();
			}
		}
		public ObservableCollection<FeatureVersion> Versions { get; } = new ObservableCollection<FeatureVersion>();

		public string TimePeriodCaption { get; } = @"Period";
		private bool _isTimePeriodsLoading;
		public bool IsTimePeriodsLoading
		{
			get { return _isTimePeriodsLoading; }
			set { this.SetField(ref _isTimePeriodsLoading, value); }
		}
		private TimePeriod _timePeriod;
		public TimePeriod TimePeriod
		{
			get { return _timePeriod; }
			set
			{
				this.SetField(ref _timePeriod, value);
				// We only need to load counts because exceptions will be the same, they won't change
				this.LoadCurrentExceptionsCounts();
			}
		}
		public ObservableCollection<TimePeriod> TimePeriods { get; } = new ObservableCollection<TimePeriod>();

		public string LatestExceptionsCaption { get; } = @"Latest exceptions";
		private bool _isExceptionsLoading;
		public bool IsExceptionsLoading
		{
			get { return _isExceptionsLoading; }
			set { this.SetField(ref _isExceptionsLoading, value); }
		}
		public ObservableCollection<FeatureExceptionEntry> LatestExceptions { get; } = new ObservableCollection<FeatureExceptionEntry>();

		public string ExceptionsCaption { get; } = @"Exceptions";
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
		public ExceptionsSettings Settings { get; }

		public ExceptionsViewModel(ITransactionContextCreator contextCreator, ExceptionsSettings settings)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			this.ContextCreator = contextCreator;
			this.Settings = settings;
		}

		public void Load(
			Func<ITransactionContext, IEnumerable<TimePeriodRow>> timePeriodsProvider,
			Func<ITransactionContext, IEnumerable<VersionRow>> versionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<FeatureExceptionEntry>> exceptionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> exceptionsCountProvider)
		{
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (exceptionsCountProvider == null) throw new ArgumentNullException(nameof(exceptionsCountProvider));
			if (timePeriodsProvider == null) throw new ArgumentNullException(nameof(timePeriodsProvider));

			this.ExceptionsProvider = exceptionsProvider;
			this.ExceptionsCountProvider = exceptionsCountProvider;

			using (var ctx = this.ContextCreator.Create())
			{
				this.LoadPeriods(ctx, timePeriodsProvider);
				this.LoadVersions(ctx, versionsProvider);

				this.TimePeriod = this.TimePeriods.FirstOrDefault();
				this.Version = this.Versions.FirstOrDefault();

				this.LoadExceptions(ctx);
				this.LoadExceptionsCounts(ctx);

				ctx.Complete();
			}

			this.IsLoaded = true;
		}

		private void LoadPeriods(ITransactionContext context, Func<ITransactionContext, IEnumerable<TimePeriodRow>> timePeriodsProvider)
		{
			this.TimePeriods.Clear();

			foreach (var timePeriod in timePeriodsProvider(context))
			{
				this.TimePeriods.Add(new TimePeriod(timePeriod));
			}
		}

		private void LoadVersions(ITransactionContext context, Func<ITransactionContext, IEnumerable<VersionRow>> versionsProvider)
		{
			this.Versions.Clear();

			foreach (var featureVersion in versionsProvider(context))
			{
				this.Versions.Add(new FeatureVersion(featureVersion));
			}
		}

		private void LoadExceptions(ITransactionContext context)
		{
			this.LatestExceptions.Clear();

			foreach (var featureException in this.ExceptionsProvider(this.GetExceptionsDataLoadParams(context)))
			{
				this.LatestExceptions.Add(featureException);
			}
		}

		private void LoadExceptionsCounts(ITransactionContext context)
		{
			this.ExceptionsCounts.Clear();

			foreach (var exceptionsCount in this.ExceptionsCountProvider(this.GetExceptionsDataLoadParams(context)))
			{
				this.ExceptionsCounts.Add(exceptionsCount);
			}
		}

		private void LoadCurrentExceptionData()
		{
			if (!this.IsLoaded) return;

			using (var ctx = this.ContextCreator.Create())
			{
				this.LoadExceptions(ctx);
				this.LoadExceptionsCounts(ctx);

				ctx.Complete();
			}
		}

		private void LoadCurrentExceptionsCounts()
		{
			if (!this.IsLoaded) return;

			using (var ctx = this.ContextCreator.Create())
			{
				this.LoadExceptionsCounts(ctx);

				ctx.Complete();
			}
		}
	}
}