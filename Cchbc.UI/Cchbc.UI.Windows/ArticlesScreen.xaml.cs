using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Cchbc.App.ArticlesModule.ViewModels;

namespace Cchbc.UI
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ArticlesScreen
	{
		private readonly ArticlesViewModel _viewModel = new ArticlesViewModel(default(Core));

		public ArticlesScreen()
		{
			this.InitializeComponent();

			this.DataContext = _viewModel;
		}

		private void ArticlesScreenOnLoaded(object sender, RoutedEventArgs e)
		{
			_viewModel.LoadData();
		}

		private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			//_viewModel.ExcludeSuppressed();
		}

		private void UIElement_OnTapped2(object sender, TappedRoutedEventArgs e)
		{
			//_viewModel.ExcludeNotInTerritory();
		}
	}
}