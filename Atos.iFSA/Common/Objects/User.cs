using System;

namespace Atos.iFSA.Common.Objects
{
	public sealed class User
	{
		public long Id { get; }
		public string Name { get; }
		public string Password { get; }

		public User(long id, string name, string password)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (password == null) throw new ArgumentNullException(nameof(password));

			this.Id = id;
			this.Name = name;
			this.Password = password;
		}
	}
}