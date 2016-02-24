using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Features.Db;

namespace Cchbc.UI
{
	public sealed class Context
	{
		public static Core Core { get; } = new Core();
	}

	public sealed partial class CommentsScreen
	{
		public LoginsViewModel ViewModel { get; } = new LoginsViewModel(Context.Core, new LoginAdapter(Context.Core.QueryHelper));

		public CommentsScreen()
		{
			this.InitializeComponent();
			this.DataContext = this.ViewModel;
		}

		private void CommentsScreenOnLoaded(object sender, RoutedEventArgs e)
		{
			this.ViewModel.LoadData();
		}

		private async void AddLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			//var btn = sender as Button;
			//if (btn == null) return;
			//{
			//	try
			//	{
			//		await this.ViewModel.AddAsync(new LoginViewModel(new Login(2, @"ZDoctor@", @"123456789", DateTime.Now, false)), new ModalDialog());
			//	}
			//	catch (Exception ex)
			//	{

			//	}
			//}
		}

		private async void DeleteLoginTapped(object sender, TappedRoutedEventArgs e)
		{
			var btn = sender as Button;
			if (btn == null) return;

			var viewModel = btn.DataContext as LoginViewModel;
			if (viewModel == null) return;

			var dialog = new ModalDialog();
			var message = @"Are you sure you want to delete this user?";
			var dialogResult = await dialog.ShowAsync(message, Feature.None, DialogType.AcceptDecline);
			if (dialogResult == DialogResult.Accept)
			{
				await this.ViewModel.DeleteAsync(viewModel, dialog);
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
					var dialog = new ModalDialog();
					var r = await dialog.ShowAsync(@"Are you sure you want to promote this user?", Feature.None, DialogType.AcceptDecline);
					if (r == DialogResult.Accept)
					{
						//await _viewModel.PromoteUserAsync(viewModel, dialog);
					}
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
