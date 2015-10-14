using Cchbc.Objects;

namespace Cchbc.UI.Comments
{
	public sealed class LoginViewItem : ViewItem<Login>
	{
		public string Name => this.Item.Name;
		public string Password => this.Item.Password;
		public string CreatedAt => this.Item.CreatedAt.ToString(@"f");
		public bool IsSystem
		{
			get { return this.Item.IsSystem; }
			set
			{
				this.Item.IsSystem = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(IsRegular));
			}
		}

		public bool IsRegular => !this.IsSystem;

		public LoginViewItem(Login login) : base(login)
		{
		}
	}
}