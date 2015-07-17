using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cchbc;

namespace Cchbc.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			// TODO : !!! Deging loggin system
			var ctx = new ArticlesContext();
			try
			{
				ILogger logger = new ConsoleBufferedLogger();
				ctx.Load(logger);

				Thread.Sleep(1000);

				//var logger = new ConsoleBufferedLogger();

				//using (var ce = new CountdownEvent(2))
				//{
				//	ThreadPool.QueueUserWorkItem(_ =>
				//	{
				//		var e = (_ as CountdownEvent);
				//		try
				//		{
				//			for (var i = 0; i < 10000; i++)
				//			{
				//				logger.Trace(i.ToString());
				//			}
				//		}
				//		finally
				//		{
				//			if (e != null)
				//			{
				//				e.Signal();
				//			}
				//		}
				//	}, ce);
				//	ThreadPool.QueueUserWorkItem(_ =>
				//	{
				//		var e = _ as CountdownEvent;
				//		try
				//		{
				//			for (var i = 10000; i < 20000; i++)
				//			{
				//				logger.Trace(i.ToString());
				//			}
				//		}
				//		finally
				//		{
				//			if (e != null)
				//			{
				//				e.Signal();
				//			}
				//		}
				//	}, ce);

				//	Thread.Sleep(5000);

				//	ce.Wait();
				//}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}

	public abstract class BufferedLogger : ILogger
	{
		protected readonly ConcurrentQueue<string> Buffer = new ConcurrentQueue<string>();

		public bool IsDebugEnabled { get; protected set; }
		public bool IsInfoEnabled { get; protected set; }
		public bool IsWarnEnabled { get; protected set; }
		public bool IsErrorEnabled { get; protected set; }

		public void Debug(string message)
		{
			if (this.IsDebugEnabled)
			{
				Buffer.Enqueue(message);
			}
		}

		public void Info(string message)
		{
			if (this.IsInfoEnabled)
			{
				Buffer.Enqueue(message);
			}
		}

		public void Warn(string message)
		{
			if (this.IsWarnEnabled)
			{
				Buffer.Enqueue(message);
			}
		}

		public void Error(string message)
		{
			if (this.IsErrorEnabled)
			{
				Buffer.Enqueue(message);
			}
		}
	}

	public sealed class ConsoleBufferedLogger : BufferedLogger
	{
		public ConsoleBufferedLogger()
		{
			this.IsInfoEnabled = true;
			this.IsWarnEnabled = true;
			this.IsErrorEnabled = true;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				while (true)
				{
					Flush();
					Thread.Sleep(100);
				}
			});
		}

		public void Flush()
		{
			var local = new StringBuilder();

			string message;
			while (Buffer.TryDequeue(out message))
			{
				local.AppendLine(message);
			}
			if (local.Length > 0)
			{
				Console.Write(local);
			}
		}
	}



	public sealed class LogLevel
	{
		public static readonly LogLevel Trace = new LogLevel(0, @"Trace");
		public static readonly LogLevel Info = new LogLevel(1, @"Info");
		public static readonly LogLevel Warn = new LogLevel(2, @"Warn");
		public static readonly LogLevel Error = new LogLevel(3, @"Error");

		public int Id { get; private set; }
		public string Name { get; private set; }

		public LogLevel(int id, string name)
		{
			if (id < 0) throw new ArgumentNullException(nameof(name));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}
