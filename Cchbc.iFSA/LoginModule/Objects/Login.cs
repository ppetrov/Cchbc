using System;

namespace Cchbc.iFSA.LoginModule.Objects
{
	public sealed class Login
	{
		public string Username { get; }
		public string Password { get; }

		public Login(string username, string password)
		{
			if (username == null) throw new ArgumentNullException(nameof(username));
			if (password == null) throw new ArgumentNullException(nameof(password));

			this.Username = username;
			this.Password = password;
		}
	}
}