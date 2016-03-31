using Cchbc.Objects;

namespace Cchbc.AppBuilder.UI.ViewModels
{
	public sealed class LoginViewModel : ViewModel<Login>
	{
		public string Name => this.Model.Name;
		public string CreatedAt => this.Model.CreatedAt.ToString(@"f");

		public string Password
		{
			get { return this.Model.Password; }
			set
			{
				this.Model.Password = value;
				this.OnPropertyChanged();
			}
		}

		public bool IsSystem
		{
			get { return this.Model.IsSystem; }
			set
			{
				this.Model.IsSystem = value;
				this.OnPropertyChanged();
			}
		}

		public LoginViewModel(Login login) : base(login)
		{
		}
	}
}