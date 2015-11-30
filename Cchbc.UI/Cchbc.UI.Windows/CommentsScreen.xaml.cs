using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.App;
using Cchbc.Features;
using Cchbc.UI.Comments;

namespace Cchbc.UI
{
	public sealed partial class CommentsScreen
	{
		private readonly LoginsViewModel _viewModel = new LoginsViewModel(Core.Current);

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
					await _viewModel.AddAsync(new LoginViewModel(new Login(2, @"ZDoctor@", @"123456789", DateTime.Now, false)), new WinRtModalDialog());
				}
				catch (Exception ex)
				{

				}
			}
		}

		private async void DeleteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				var viewModel = btn.DataContext as LoginViewModel;
				if (viewModel != null)
				{
					var dialog = new WinRtModalDialog();
					dialog.AcceptAction = async () => { await _viewModel.DeleteAsync(viewModel, dialog); };
					await dialog.ConfirmAsync(@"Are you sure you want to delete this user?", Feature.None);
				}
			}
		}

		private async void PromoteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
			{
				var viewModel = btn.DataContext as LoginViewModel;
				if (viewModel != null)
				{
					var dialog = new WinRtModalDialog();
					dialog.AcceptAction = async () =>
					{
						await _viewModel.PromoteUserAsync(viewModel, dialog);
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
