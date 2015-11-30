using Cchbc.Objects;

namespace Cchbc.UI
{
	public sealed class LoginViewModel : ViewModel<Login>
	{
		public string Name => this.Model.Name;
		public string Password => this.Model.Password;
		public string CreatedAt => this.Model.CreatedAt.ToString(@"f");
		public bool IsSystem
		{
			get { return this.Model.IsSystem; }
			set
			{
				this.Model.IsSystem = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(IsRegular));
			}
		}

		public bool IsRegular => !this.IsSystem;

		public LoginViewModel(Login login) : base(login)
		{
		}
	}
}