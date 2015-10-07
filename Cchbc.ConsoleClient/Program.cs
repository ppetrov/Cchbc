using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.ConsoleClient
{
	//public sealed class ArticleViewItemSortOption : SortOption<ArticleViewItem>
	//{
	//    public static readonly Func<ArticleViewItem, ArticleViewItem, int> ByDefault = (x, y) =>
	//    {
	//        var cmp = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
	//        return cmp;
	//    };

	//    public static readonly Func<ArticleViewItem, ArticleViewItem, int> ByBrand = (x, y) =>
	//    {
	//        var cmp = string.Compare(x.Brand, y.Brand, StringComparison.Ordinal);
	//        if (cmp == 0)
	//        {
	//            cmp = ByDefault(x, y);
	//        }
	//        return cmp;
	//    };

	//    public static readonly Func<ArticleViewItem, ArticleViewItem, int> ByFlavor = (x, y) =>
	//    {
	//        var cmp = string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal);
	//        if (cmp == 0)
	//        {
	//            cmp = ByDefault(x, y);
	//        }
	//        return cmp;
	//    };

	//    public ArticleViewItemSortOption(string displayName, Func<ArticleViewItem, ArticleViewItem, int> comparison)
	//        : base(displayName, comparison)
	//    {
	//    }
	//}

	

	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				//var adapter = new CommentAdapter();
				//var m = new CommentsManager(adapter,
				//	new Sorter<CommentViewItem>(new[]
				//{
				//	new SortOption<CommentViewItem>(@"By Type", (x,y)=>string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)),
				//}), new Searcher<CommentViewItem>(new[]
				//{
				//	new SearchOption<CommentViewItem>(@"All", v=> true),
				//}));
				//m.LoadData(new List<CommentViewItem>());

				//m.ItemInserted += (sender, eventArgs) =>
				//{
				//	Console.WriteLine(@"Item successfully inserted!");
				//	Console.WriteLine(@"Add additional logic");
				//};

				//var dialog = new DebugModalDialog();
    //            m.AddAsync(new CommentViewItem(new Comment()), dialog).Wait();
				//Console.WriteLine();
				return;

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
					Thread.Sleep(20);
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
