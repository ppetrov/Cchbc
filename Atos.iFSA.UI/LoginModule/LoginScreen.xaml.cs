using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Atos.iFSA.LoginModule;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed partial class LoginScreen
	{
		public LoginScreenViewModel ViewModel { get; } = new LoginScreenViewModel(GlobalApp.Context);

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
