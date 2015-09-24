using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Cchbc.Search;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Cchbc.UI
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private readonly ArticlesViewModel _viewModel = new ArticlesViewModel(new DirectDebugLogger());

		public MainPage()
		{
			this.InitializeComponent();

			this.DataContext = _viewModel;
		}

		private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			_viewModel.Load();
		}

		private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
		{
			_viewModel.SearchOption = e.ClickedItem as SearchOption<ArticleViewItem>;
		}
	}

	public abstract class BufferedLogger : ILogger
	{
		//protected readonly ConcurrentQueue<string> Buffer = new ConcurrentQueue<string>();

		public bool IsDebugEnabled { get; protected set; }
		public bool IsInfoEnabled { get; protected set; }
		public bool IsWarnEnabled { get; protected set; }
		public bool IsErrorEnabled { get; protected set; }

		public void Debug(string message)
		{
			if (this.IsDebugEnabled)
			{
				System.Diagnostics.Debug.WriteLine(message);
			}
		}

		public void Info(string message)
		{
			if (this.IsInfoEnabled)
			{
				System.Diagnostics.Debug.WriteLine(message);
			}
		}

		public void Warn(string message)
		{
			if (this.IsWarnEnabled)
			{
				System.Diagnostics.Debug.WriteLine(message);
			}
		}

		public void Error(string message)
		{
			if (this.IsErrorEnabled)
			{
				System.Diagnostics.Debug.WriteLine(message);
			}
		}
	}

	public sealed class DirectDebugLogger : BufferedLogger
	{
		public DirectDebugLogger()
		{
			this.IsDebugEnabled = true;
			this.IsInfoEnabled = true;
			this.IsWarnEnabled = true;
			this.IsErrorEnabled = true;
		}

		public void Flush()
		{
			var local = new StringBuilder();

			//string message;
			//while (Buffer.TryDequeue(out message))
			//{
			//    local.AppendLine(message);
			//}
			//if (local.Length > 0)
			//{
			//    Console.Write(local);
			//}
		}
	}
}
