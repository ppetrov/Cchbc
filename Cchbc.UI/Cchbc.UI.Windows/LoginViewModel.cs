using Cchbc.Objects;

namespace Cchbc.UI
{
	public sealed class LoginViewModel : ViewModel<Login>
	{
		public string Name => this.Model.Name;

		private string _password = string.Empty;
		public string Password
		{
			get { return _password; }
			set { this.SetField(ref _password, value); }
		}

		public string CreatedAt => this.Model.CreatedAt.ToString(@"f");

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