using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Cchbc.UI.Comments;

namespace Cchbc.UI
{
	public sealed partial class CommentsScreen
	{
		private readonly CommentsViewModel _viewModel = new CommentsViewModel(new DirectDebugLogger());

		public CommentsScreen()
		{
			this.InitializeComponent();
		}

		private async void CommentsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			await _viewModel.LoadDataAsync();
		}

		private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			try
			{
				this.BtnAdd.IsEnabled = false;

				await _viewModel.AddAsync();
			}
			finally
			{
				this.BtnAdd.IsEnabled = true;
			}
		}
	}
}
