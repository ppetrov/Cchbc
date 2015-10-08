using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.Dialog;
using Cchbc.UI.Comments;

namespace Cchbc.UI
{
	public sealed partial class CommentsScreen
	{
		private readonly LoginsViewModel _viewModel = new LoginsViewModel(new DirectDebugLogger());

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
			await _viewModel.AddAsync(new LoginViewItem(new Login(2, @"Doctor@", @"123456789", DateTime.Now, false)), new WinRtModalDialog());
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
					await dialog.ConfirmAsync(@"Are you sure you want to delete this user?");
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
					dialog.AcceptAction = async () => { await _viewModel.PromoteAsync(viewItem, dialog); };
					await dialog.ConfirmAsync(@"Are you sure you want to promote this user?");
				}
			}
		}

		private async void UIElement_OnTapped3(object sender, TappedRoutedEventArgs e)
		{
			//var dialog = new WinRtModalDialog();
			//dialog.AcceptAction = async () => { await _viewModel.MarkAsync(dialog); };
			//await dialog.ShowAsync(@"Are you sure you want to mark as read ?");
		}

		
	}
}
