using Windows.UI.Xaml;

namespace Cchbc.UI
{
	public sealed partial class CommentsScreen
	{
		//private readonly CommentsViewModel _viewModel = new CommentsViewModel(new DirectDebugLogger());

		public CommentsScreen()
		{
			this.InitializeComponent();
		}

		private async void CommentsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			//await _viewModel.LoadDataAsync();
		}
	}
}
