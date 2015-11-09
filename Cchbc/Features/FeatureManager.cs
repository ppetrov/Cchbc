using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cchbc.Features
{
	public sealed class FeatureManager : IDisposable
	{
		private readonly int _bufferSize;
		private readonly Task _syncTask;

		public FeatureManager(Action<FeatureEntry[]> dumper, int bufferSize = 16)
		{
			_bufferSize = bufferSize;
			if (dumper == null) throw new ArgumentNullException(nameof(dumper));

			_syncTask = Task.Run(() =>
			{
				var buffer = new List<FeatureEntry>();

				foreach (var entry in this.Entries.GetConsumingEnumerable())
				{
					buffer.Add(entry);

					if (buffer.Count >= _bufferSize)
					{
						dumper(buffer.ToArray());
						buffer.Clear();
					}
				}
				if (buffer.Any())
				{
					dumper(buffer.ToArray());
				}
			});
		}

		private BlockingCollection<FeatureEntry> Entries { get; } = new BlockingCollection<FeatureEntry>();

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

			this.Entries.TryAdd(new FeatureEntry(feature.Context, feature.Name, feature.Details, feature.TimeSpent, steps));
		}

		public void Dispose()
		{
			// Signal the end of adding any entries
			this.Entries.CompleteAdding();

			// Wait for the sync task to complete(flush all the entries)
			_syncTask.Wait();
		}
	}
}