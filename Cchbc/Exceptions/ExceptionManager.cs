using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cchbc.Exceptions
{
	public sealed class ExceptionManager
	{
		private readonly int _bufferSize;
		private readonly Task _syncTask;

		public ExceptionManager(Action<ExceptionEntry[]> dumper, int bufferSize = 16)
		{
			_bufferSize = bufferSize;
			if (dumper == null) throw new ArgumentNullException(nameof(dumper));

			_syncTask = Task.Run(() =>
			{
				var buffer = new List<ExceptionEntry>();

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

		private BlockingCollection<ExceptionEntry> Entries { get; } = new BlockingCollection<ExceptionEntry>();

		public void Add(ExceptionEntry entry)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));

            this.Entries.TryAdd(entry);
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