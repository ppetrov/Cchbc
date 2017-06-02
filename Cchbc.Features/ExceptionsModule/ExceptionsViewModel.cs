using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Atos.Data;
using Atos.Features.ExceptionsModule.Objects;
using Atos.Features.ExceptionsModule.Rows;

namespace Atos.Features.ExceptionsModule
{

	public sealed class ExceptionsCountViewModel
	{
		public ExceptionsCount Model { get; }

		public string CreatedAtFormatted { get; }
		public int Count { get; }

		public ExceptionsCountViewModel(ExceptionsCount model)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));

			this.Model = model;
			this.Count = model.Count;
			this.CreatedAtFormatted = model.DateTime.ToString(@"d");
		}
	}

	public sealed class ExceptionsViewModel : ViewModel
	{
		private bool IsLoaded { get; set; }

		private ExceptionsDataLoadParams GetExceptionsDataLoadParams(IDbContext context, ExceptionsSettings settings)
		{
			var loadParams = new ExceptionsDataLoadParams(context, settings)
			{
				Version = this.Version,
				TimePeriod = this.TimePeriod
			};
			return loadParams;
		}

		private bool? _removeExcluded = true;
		public bool? RemoveExcluded
		{
			get { return _removeExcluded; }
			set
			{
				this.SetProperty(ref _removeExcluded, value);

				this.LoadCurrentExceptionData(new ExceptionsSettings(this.Settings.MaxExceptionEntries, value ?? false));
			}
		}

		public string VersionCaption { get; } = @"Version";
		private bool _isVersionsLoading;
		public bool IsVersionsLoading
		{
			get { return _isVersionsLoading; }
			set { this.SetProperty(ref _isVersionsLoading, value); }
		}
		private FeatureVersion _version;
		public FeatureVersion Version
		{
			get { return _version; }
			set
			{
				this.SetProperty(ref _version, value);
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
			set { this.SetProperty(ref _isTimePeriodsLoading, value); }
		}
		private TimePeriod _timePeriod;
		public TimePeriod TimePeriod
		{
			get { return _timePeriod; }
			set
			{
				this.SetProperty(ref _timePeriod, value);
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
			set { this.SetProperty(ref _isExceptionsLoading, value); }
		}
		public ObservableCollection<FeatureExceptionEntry> LatestExceptions { get; } = new ObservableCollection<FeatureExceptionEntry>();

		public string ExceptionsCaption { get; } = @"Exceptions";
		private bool _isExceptionsCountsLoading;
		public bool IsExceptionsCountsLoading
		{
			get { return _isExceptionsCountsLoading; }
			set { this.SetProperty(ref _isExceptionsCountsLoading, value); }
		}
		public ObservableCollection<ExceptionsCountViewModel> ExceptionsCounts { get; } = new ObservableCollection<ExceptionsCountViewModel>();

		public Func<IDbContext> DbContextCreator { get; }
		public Func<ExceptionsDataLoadParams, IEnumerable<FeatureExceptionEntry>> ExceptionsProvider { get; private set; }
		public Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> ExceptionsCountProvider { get; private set; }
		public ExceptionsSettings Settings { get; }

		public ExceptionsViewModel(Func<IDbContext> dbContextCreator, ExceptionsSettings settings)
		{
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			this.DbContextCreator = dbContextCreator;
			this.Settings = settings;
		}

		public void Load(
			Func<IDbContext, IEnumerable<TimePeriodRow>> timePeriodsProvider,
			Func<IDbContext, IEnumerable<VersionRow>> versionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<FeatureExceptionEntry>> exceptionsProvider,
			Func<ExceptionsDataLoadParams, IEnumerable<ExceptionsCount>> exceptionsCountProvider)
		{
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (exceptionsCountProvider == null) throw new ArgumentNullException(nameof(exceptionsCountProvider));
			if (timePeriodsProvider == null) throw new ArgumentNullException(nameof(timePeriodsProvider));

			this.ExceptionsProvider = exceptionsProvider;
			this.ExceptionsCountProvider = exceptionsCountProvider;

			using (var ctx = this.DbContextCreator())
			{
				this.LoadPeriods(ctx, timePeriodsProvider);
				this.LoadVersions(ctx, versionsProvider);

				this.TimePeriod = this.TimePeriods.FirstOrDefault();
#if DEBUG
				this.TimePeriod = this.TimePeriods.LastOrDefault();
#endif
				this.Version = this.Versions.FirstOrDefault();

				this.LoadExceptions(ctx);
				this.LoadExceptionsCounts(ctx);

				ctx.Complete();
			}

			this.IsLoaded = true;
		}

		private void LoadPeriods(IDbContext context, Func<IDbContext, IEnumerable<TimePeriodRow>> timePeriodsProvider)
		{
			this.TimePeriods.Clear();

			foreach (var timePeriod in timePeriodsProvider(context))
			{
				this.TimePeriods.Add(new TimePeriod(timePeriod));
			}
		}

		private void LoadVersions(IDbContext context, Func<IDbContext, IEnumerable<VersionRow>> versionsProvider)
		{
			this.Versions.Clear();

			foreach (var featureVersion in versionsProvider(context))
			{
				this.Versions.Add(new FeatureVersion(featureVersion));
			}
		}

		private void LoadExceptions(IDbContext context, ExceptionsSettings settings = null)
		{
			this.LatestExceptions.Clear();

			foreach (var featureException in this.ExceptionsProvider(this.GetExceptionsDataLoadParams(context, settings ?? this.Settings)))
			{
				this.LatestExceptions.Add(featureException);
			}
		}

		private void LoadExceptionsCounts(IDbContext context, ExceptionsSettings settings = null)
		{
			this.ExceptionsCounts.Clear();

			foreach (var exceptionsCount in this.ExceptionsCountProvider(this.GetExceptionsDataLoadParams(context, settings ?? this.Settings)))
			{
				this.ExceptionsCounts.Add(new ExceptionsCountViewModel(exceptionsCount));
			}
		}

		private void LoadCurrentExceptionData(ExceptionsSettings settings = null)
		{
			if (!this.IsLoaded) return;

			using (var ctx = this.DbContextCreator())
			{
				this.LoadExceptions(ctx, settings);
				this.LoadExceptionsCounts(ctx, settings);

				ctx.Complete();
			}
		}

		private void LoadCurrentExceptionsCounts()
		{
			if (!this.IsLoaded) return;

			using (var ctx = this.DbContextCreator())
			{
				this.LoadExceptionsCounts(ctx);

				ctx.Complete();
			}
		}
	}
}