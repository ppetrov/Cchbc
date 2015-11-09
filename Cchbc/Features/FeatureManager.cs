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
		private Task _featuresTask;
		private Task _exceptionsTask;
		private BlockingCollection<FeatureEntry> _features;
		private BlockingCollection<ExceptionEntry> _exceptions;
		private IDbFeaturesManager _dbFeaturesManager;

		public void Initialize(ILogger logger, IDbFeaturesManager dbFeaturesManager)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (dbFeaturesManager == null) throw new ArgumentNullException(nameof(dbFeaturesManager));

			_logger = logger;
			_dbFeaturesManager = dbFeaturesManager;
			_features = new BlockingCollection<FeatureEntry>();
			_exceptions = new BlockingCollection<ExceptionEntry>();
		}

		public void StartDbWriters()
		{
			_featuresTask = Task.Run(async () =>
			{
				foreach (var entry in _features.GetConsumingEnumerable())
				{
					try
					{
						await _dbFeaturesManager.SaveAsync(entry);
					}
					catch (Exception ex) { _logger.Error(ex.ToString()); }
				}
			});
			_exceptionsTask = Task.Run(async () =>
			{
				foreach (var entry in _exceptions.GetConsumingEnumerable())
				{
					try
					{
						await _dbFeaturesManager.SaveAsync(entry);
					}
					catch (Exception ex) { _logger.Error(ex.ToString()); }
				}
			});
		}

		public void StopDbWriters()
		{
			_featuresTask.Wait();
			_exceptionsTask.Wait();

			_features.CompleteAdding();
			_exceptions.CompleteAdding();

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

		public void Stop(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Ignore None(empty) features
			if (feature == Feature.None)
			{
				return;
			}

			feature.EndStep();
			feature.StopMeasure();

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

			_features.TryAdd(new FeatureEntry(feature.Context, feature.Name, feature.Details, feature.TimeSpent, steps));
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			_exceptions.TryAdd(new ExceptionEntry(feature.Context, feature.Name, exception));
		}
	}
}