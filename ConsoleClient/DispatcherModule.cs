using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Oracle.ManagedDataAccess.Client;

namespace ConsoleClient
{
	public sealed class ServiceConfig
	{
		public int Workers { get; }
		public TimeSpan QueryItemsInterval { get; }
		public string ConnectionString { get; }

		public ServiceConfig(int workers, TimeSpan queryItemsInterval, string connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			this.Workers = workers;
			this.QueryItemsInterval = queryItemsInterval;
			this.ConnectionString = connectionString;
		}
	}

	public sealed class DispatchItemResult
	{
	}

	public abstract class Dispatcher<T> : IDisposable
	{
		private ManualResetEventSlim ExitRequested { get; }
		private ManualResetEventSlim IsCompleted { get; }
		private Action<Exception> Log { get; }

		protected Dispatcher(Action<Exception> log)
		{
			if (log == null) throw new ArgumentNullException(nameof(log));

			this.Log = log;
			this.ExitRequested = new ManualResetEventSlim(false);
			this.IsCompleted = new ManualResetEventSlim(false);
		}

		public void Start()
		{
			while (!this.ExitRequested.IsSet)
			{
				var s = Stopwatch.StartNew();

				var config = GetConfig();
				try
				{
					using (var cn = new OracleConnection(config.ConnectionString))
					{
						cn.Open();
						var items = this.GetItems(cn);
						if (items.Count > 0)
						{
							using (var ce = new CountdownEvent(Math.Min(config.Workers, items.Count)))
							{
								var parameters = new object[] { ce, items, config, cn };

								for (var i = 0; i < ce.InitialCount; i++)
								{
									ThreadPool.QueueUserWorkItem(this.Dispatch, parameters);
								}

								ce.Wait();
							}
						}
					}
				}
				catch (Exception ex)
				{
					this.Log(ex);
				}

				var sleepTime = config.QueryItemsInterval - s.Elapsed;
				if (sleepTime >= TimeSpan.Zero)
				{
					this.ExitRequested.Wait(sleepTime);
				}
			}

			this.IsCompleted.Set();
		}

		public void Stop()
		{
			// Request exit
			this.ExitRequested.Set();

			// Wait while it completes
			this.IsCompleted.Wait();
		}

		public abstract ServiceConfig GetConfig();

		public abstract ConcurrentQueue<T> GetItems(OracleConnection connection);

		public abstract DispatchItemResult Dispatch(ServiceConfig config, T item);

		public abstract void Mark(OracleConnection cn, T item, DispatchItemResult result);

		private void Dispatch(object state)
		{
			var args = state as object[];
			var e = args[0] as CountdownEvent;
			var items = args[1] as ConcurrentQueue<T>;
			var config = args[2] as ServiceConfig;
			var cn = args[3] as OracleConnection;
			try
			{
				T item;
				while (items.TryDequeue(out item))
				{
					if (this.ExitRequested.IsSet)
					{
						break;
					}
					try
					{
						var result = this.Dispatch(config, item);
						lock (this)
						{
							this.Mark(cn, item, result);
						}
					}
					catch (Exception ex)
					{
						this.Log(ex);
					}
				}
			}
			finally
			{
				e.Signal();
			}
		}

		public void Dispose()
		{
			this.ExitRequested.Dispose();
			this.IsCompleted.Dispose();
		}
	}
}