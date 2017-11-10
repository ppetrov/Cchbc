using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Atos.iFSA.LoginModule;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed partial class LoginScreen
	{
		public LoginPageViewModel ViewModel { get; } = new LoginPageViewModel(GlobalApp.Context);

		public LoginScreen()
		{
			this.InitializeComponent();
		}

		private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Password = (sender as PasswordBox).Password;
		}
	}
}
