using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Atos.iFSA.LoginModule;
using Atos.iFSA.LoginModule.Data;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed partial class LoginScreen
	{
		public LoginScreenViewModel ViewModel { get; } = new LoginScreenViewModel(GlobalApp.Context, GlobalApp.AppNavigator, new LoginScreenDataProvider(new SettingsProvider()));

		public LoginScreen()
		{
			this.InitializeComponent();
		}

		private void LoginScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			this.ViewModel.LoadData();
		}

		private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Password = (sender as PasswordBox).Password;
		}
	}
}
