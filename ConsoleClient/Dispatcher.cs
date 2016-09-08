using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleClient
{
	public sealed class DispatcherSettings
	{
		public int Workers { get; }
		public TimeSpan QueryItemsInterval { get; }

		public DispatcherSettings(int workers, TimeSpan queryItemsInterval)
		{
			this.QueryItemsInterval = queryItemsInterval;
			Workers = workers;
		}
	}

	public sealed class Dispatcher
	{
		private readonly ManualResetEventSlim _exitEvent = new ManualResetEventSlim(false);

		public void Dispatch<T>(DispatcherSettings settings, Func<List<T>> sourceItemsProvider, Action<T> resultProcessor)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (sourceItemsProvider == null) throw new ArgumentNullException(nameof(sourceItemsProvider));
			if (resultProcessor == null) throw new ArgumentNullException(nameof(resultProcessor));

			using (_exitEvent)
			{
				while (!_exitEvent.IsSet)
				{
					// Get the items from the provider
					var items = new ConcurrentQueue<T>(sourceItemsProvider());
					if (!items.Any())
					{
						// We don't have items. Wait for items
						if (_exitEvent.Wait(settings.QueryItemsInterval))
						{
							break;
						}
					}

					// Process items in parallel
					using (var ce = new CountdownEvent(Math.Min(settings.Workers, items.Count)))
					{
						for (var i = 0; i < ce.InitialCount; i++)
						{
							ThreadPool.QueueUserWorkItem(_ =>
							{
								var parameters = _ as object[];
								var source = parameters[0] as ConcurrentQueue<T>;
								var finishEvent = parameters[1] as CountdownEvent;
								var exitEvent = parameters[2] as ManualResetEventSlim;

								try
								{
									T item;
									while (source.TryDequeue(out item) && !exitEvent.IsSet)
									{
										try
										{
											resultProcessor(item);
										}
										catch
										{
											// TODO : Unable to process item item. Log exception
										}
									}
								}
								finally
								{
									finishEvent.Signal();
								}

							}, new object[] { items, ce, _exitEvent });
						}

						ce.Wait();
					}
				}
			}
		}
	}
}