using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.UI.Comments;

namespace Cchbc.UI
{
	public static class CurrentApp
	{
		private static Core Core { get; set; }

		public static Core GetCore()
		{
			if (Core == null)
			{
				Core = new Core(new DirectDebugLogger(@"N/A"), new QueryHelper(null, null));
				Core.FeatureManager = new FeatureManager(entries =>
				{
					foreach (var entry in entries)
					{
						Debug.WriteLine(entry.Context + ":" + entry.Name.Replace(@"Async", ""));
						foreach (var s in entry.Steps)
						{
							Debug.WriteLine("\t" + s.Name.Replace("Async", "") + " " + s.TimeSpent.TotalMilliseconds + " ms");
						}
						Debug.WriteLine(string.Empty);
					}
				}, 1);
			}

			return Core;
		}

	}

	public sealed partial class CommentsScreen
	{
		private readonly LoginViewModel _viewModel = new LoginViewModel(CurrentApp.GetCore());

		public CommentsScreen()
		{
			this.InitializeComponent();
			this.DataContext = _viewModel;
		}

		private async void CommentsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			await _viewModel.LoadDataAsync();
		}

		private async void AddLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				try
				{
					await _viewModel.AddAsync(new LoginViewItem(new Login(2, @"ZDoctor@", @"123456789", DateTime.Now, false)), new WinRtModalDialog());
				}
				catch (Exception ex)
				{
					// TODO : !!! Log the exception
					//_viewModel.
				}
			}
		}

		private async void DeleteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				var viewItem = btn.DataContext as LoginViewItem;
				if (viewItem != null)
				{
					var dialog = new WinRtModalDialog();
					dialog.AcceptAction = async () => { await _viewModel.DeleteAsync(viewItem, dialog); };
					await dialog.ConfirmAsync(@"Are you sure you want to delete this user?", Feature.None);
				}
			}
		}

		private async void PromoteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				var viewItem = btn.DataContext as LoginViewItem;
				if (viewItem != null)
				{
					var dialog = new WinRtModalDialog();
					dialog.AcceptAction = async () =>
					{
						await _viewModel.PromoteUserAsync(viewItem, dialog);
					};
					await dialog.ConfirmAsync(@"Are you sure you want to promote this user?", Feature.None);
				}
			}
		}

		private void UIElement_OnTapped3(object sender, TappedRoutedEventArgs e)
		{
			//var dialog = new WinRtModalDialog();
			//dialog.AcceptAction = async () => { await _viewModel.MarkAsync(dialog); };
			//await dialog.ShowAsync(@"Are you sure you want to mark as read ?");
		}
	}
}
