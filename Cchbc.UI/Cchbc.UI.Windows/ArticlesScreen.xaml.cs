using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.App;
using Cchbc.App.Articles.ViewModel;
using Cchbc.Search;

namespace Cchbc.UI
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ArticlesScreen : Page
	{
		private readonly ArticlesViewModel _viewModel = new ArticlesViewModel(default(Core));

		public ArticlesScreen()
		{
			this.InitializeComponent();

			this.DataContext = _viewModel;
		}

		private async void ArticlesScreenOnLoaded(object sender, RoutedEventArgs e)
		{
			await _viewModel.LoadDataAsync();
		}

		private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			_viewModel.ExcludeSuppressed();
		}

		private void UIElement_OnTapped2(object sender, TappedRoutedEventArgs e)
		{
			_viewModel.ExcludeNotInTerritory();
		}

	}

	public abstract class BufferedLogger : ILogger
	{
		public string Context { get; }

		protected BufferedLogger(string context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
		}

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
				System.Diagnostics.Debug.WriteLine(this.Context + ":" + message);
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
		public DirectDebugLogger(string context) : base(context)
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
