using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cchbc
{
	public sealed class FeatureManager : IDisposable
	{
		private readonly int _bufferSize;
		private readonly Task _syncTask;

		public FeatureManager(Action<Feature[]> dumper, int bufferSize = 256)
		{
			_bufferSize = bufferSize;
			if (dumper == null) throw new ArgumentNullException(nameof(dumper));

			_syncTask = Task.Run(() =>
			{
				var buffer = new List<Feature>();

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

		private BlockingCollection<Feature> Entries { get; } = new BlockingCollection<Feature>();

		public Feature StartNew(string context, string name, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var feature = new Feature(context, name, details ?? string.Empty);

			feature.StartMeasure();

			return feature;
		}

		public void Stop(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.EndStep();
			feature.StopMeasure();

			// Ignore None(empty) features
			if (feature == Feature.None)
			{
				return;
			}
			try
			{
				this.Entries.TryAdd(feature);
			}
			catch { }
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