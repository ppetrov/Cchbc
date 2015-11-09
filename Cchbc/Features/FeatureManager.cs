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
		private readonly Task _featuresTask;
		private readonly Task _exceptionsTask;

		private BlockingCollection<FeatureEntry> Features { get; } = new BlockingCollection<FeatureEntry>();
		private BlockingCollection<ExceptionEntry> Exceptions { get; } = new BlockingCollection<ExceptionEntry>();

		public FeatureManager(Action<ExceptionEntry[]> exceptionsDbWriter, Action<FeatureEntry[]> featuresDbWriter, int bufferSize = 8)
		{
			if (exceptionsDbWriter == null) throw new ArgumentNullException(nameof(exceptionsDbWriter));
			if (featuresDbWriter == null) throw new ArgumentNullException(nameof(featuresDbWriter));

			_bufferSize = bufferSize;

			_featuresTask = Task.Run(() =>
			{
				var buffer = new List<FeatureEntry>();

				foreach (var entry in this.Features.GetConsumingEnumerable())
				{
					buffer.Add(entry);

					if (buffer.Count >= _bufferSize)
					{
						featuresDbWriter(buffer.ToArray());
						buffer.Clear();
					}
				}
				if (buffer.Any())
				{
					featuresDbWriter(buffer.ToArray());
				}
			});

			_exceptionsTask = Task.Run(() =>
			{
				foreach (var entry in this.Exceptions.GetConsumingEnumerable())
				{
					exceptionsDbWriter(new[] { entry });
				}
			});
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

			this.Features.TryAdd(new FeatureEntry(feature.Context, feature.Name, feature.Details, feature.TimeSpent, steps));
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Exceptions.TryAdd(new ExceptionEntry(feature.Context, feature.Name, exception));
		}

		public void Dispose()
		{
			// Signal the end of adding any entries
			this.Features.CompleteAdding();
			this.Exceptions.CompleteAdding();

			// Wait for the sync tasks to complete(flush all the entries)
			Task.WaitAll(_featuresTask, _exceptionsTask);
		}
	}
}