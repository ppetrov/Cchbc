using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Features.Db;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private ILogger _logger;
		private DbFeaturesManager _dbFeaturesManager;

		private Task _featuresTask;
		private Task _exceptionsTask;
		private BlockingCollection<FeatureEntry> _features;
		private BlockingCollection<ExceptionEntry> _exceptions;

		public Action<FeatureEntry> InspectFeature { get; set; }

		public void Load(ILogger logger, DbFeaturesManager dbFeaturesManager)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (dbFeaturesManager == null) throw new ArgumentNullException(nameof(dbFeaturesManager));

			_logger = logger;
			_dbFeaturesManager = dbFeaturesManager;

			_dbFeaturesManager.Load();
		}

		public void StartDbWriters()
		{
			_features = new BlockingCollection<FeatureEntry>();
			_exceptions = new BlockingCollection<ExceptionEntry>();

			_featuresTask = Task.Run(() =>
			{
				foreach (var entry in _features.GetConsumingEnumerable())
				{
					try
					{
						this.InspectFeature?.Invoke(entry);
						_dbFeaturesManager.Save(entry);
					}
					catch (Exception ex) { _logger.Error(ex.ToString()); }
				}
			});
			_exceptionsTask = Task.Run(() =>
			{
				foreach (var entry in _exceptions.GetConsumingEnumerable())
				{
					try
					{
						_dbFeaturesManager.Save(entry);
					}
					catch (Exception ex) { _logger.Error(ex.ToString()); }
				}
			});
		}

		public void StopDbWriters()
		{
			_features.CompleteAdding();
			_exceptions.CompleteAdding();

			_featuresTask.Wait();
			_exceptionsTask.Wait();

			_features = null;
			_exceptions = null;
			_featuresTask = null;
			_exceptionsTask = null;
		}

		public void Start(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.StartMeasure();
		}

		public void Stop(Feature feature, Exception exception = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Exceptions have higher priority and will be logged even for None features
			ExceptionEntry exceptionEntry = null;
			if (exception != null)
			{
				exceptionEntry = new ExceptionEntry(feature.Context, feature.Name, exception);
				_exceptions.TryAdd(exceptionEntry);
			}

			// Ignore None(empty) features without exception
			if (feature == Feature.None)
			{
				return;
			}

			this.Finish(feature, exceptionEntry);
		}

		private void Finish(Feature feature, ExceptionEntry exceptionEntry)
		{
			feature.FinishMeasure();

			var steps = Enumerable.Empty<FeatureEntryStep>().ToArray();

			var featureSteps = feature.Steps;
			if (featureSteps.Count > 0)
			{
				steps = new FeatureEntryStep[featureSteps.Count];

				for (var i = 0; i < steps.Length; i++)
				{
					steps[i] = new FeatureEntryStep(featureSteps[i].Name, featureSteps[i].TimeSpent, featureSteps[i].Details);
				}
			}

			_features.TryAdd(new FeatureEntry(feature.Context, feature.Name, feature.Details, feature.TimeSpent, steps, exceptionEntry));
		}
	}
}