using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Cchbc
{
	public sealed class FeatureManager : IDisposable
	{
		private readonly Task _syncTask;

		public FeatureManager(Action<BlockingCollection<FeatureEntry>> dumper)
		{
			_syncTask = Task.Run(() => { dumper(this.Entries); });
		}

		private BlockingCollection<FeatureEntry> Entries { get; } = new BlockingCollection<FeatureEntry>();

		public void Add(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.StopMeasure();

			// Ignore None(empty) features
			if (feature == Feature.None)
			{
				return;
			}
			try
			{
				this.Entries.TryAdd(new FeatureEntry(feature.Context, feature.Name, feature.TimeSpent));
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