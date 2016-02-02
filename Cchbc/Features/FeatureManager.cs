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

			feature.Start();
		}

		public void Stop(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Complete(feature, details);
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			_exceptions.TryAdd(new ExceptionEntry(feature.Context, feature.Name, exception));
		}

		private void Complete(Feature feature, string details)
		{
			// Stop the feature
			feature.Stop();

			var steps = Enumerable.Empty<FeatureEntryStep>().ToArray();

			var featureSteps = feature.Steps;
			if (featureSteps.Count > 0)
			{
				steps = new FeatureEntryStep[featureSteps.Count];

				for (var i = 0; i < steps.Length; i++)
				{
					var s = featureSteps[i];
					steps[i] = new FeatureEntryStep(s.Name, s.TimeSpent, s.Details);
				}
			}

			_features.TryAdd(new FeatureEntry(feature.Context, feature.Name, details ?? string.Empty, feature.TimeSpent, steps));
		}

		public Feature StartNew(string context, string name)
		{
			return new Feature(context, name);
		}
	}
}