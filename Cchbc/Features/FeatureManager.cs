using System;
using System.Linq;
using Cchbc.Features.Db;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private DbFeaturesManager _dbManager;

		public void Init(DbFeaturesManager dbFeaturesManager)
		{
			if (dbFeaturesManager == null) throw new ArgumentNullException(nameof(dbFeaturesManager));

			if (_dbManager == null)
			{
				_dbManager = dbFeaturesManager;
				_dbManager.Load();
			}
		}

		public void Start(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.Start();
		}

		public void Stop(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

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

			_dbManager.Save(new FeatureEntry(feature.Context, feature.Name, details ?? string.Empty, feature.TimeSpent, steps));
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			_dbManager.Save(new ExceptionEntry(feature.Context, feature.Name, exception));
		}

		public Feature StartNew(string context, string name)
		{
			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}
	}
}