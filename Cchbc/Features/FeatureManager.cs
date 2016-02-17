using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Features.Db;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private Task _featuresTask;
		private DbFeaturesManager _dbManager;
		private BlockingCollection<DbEntry> _entries;

		public void Init(DbFeaturesManager dbFeaturesManager)
		{
			if (dbFeaturesManager == null) throw new ArgumentNullException(nameof(dbFeaturesManager));

			if (_dbManager == null)
			{
				_dbManager = dbFeaturesManager;
				_dbManager.Load();
			}

			_entries = new BlockingCollection<DbEntry>();
			_featuresTask = Task.Run(() =>
			{
				foreach (var entry in this._entries.GetConsumingEnumerable())
				{
					try
					{
						_dbManager.Save(entry);
					}
					catch { }
				}
			});
		}

		public void Flush()
		{
			if (_featuresTask == null) return;
			
			// Signal the end of adding entries
			_entries.CompleteAdding();

			// Wait for all the entries to be saved in the database
			_featuresTask.Wait();
			_featuresTask = null;
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

			this._entries.TryAdd(new FeatureEntry(feature.Context, feature.Name, details ?? string.Empty, feature.TimeSpent, steps));
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this._entries.TryAdd(new ExceptionEntry(feature.Context, feature.Name, exception));
		}

		public Feature StartNew(string context, string name)
		{
			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}
	}
}