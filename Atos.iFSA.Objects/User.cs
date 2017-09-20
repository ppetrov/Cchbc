using System;

namespace Atos.iFSA.Objects
{
	public sealed class User
	{
		public long Id { get; }
		public string Name { get; }
		public string Password { get; }
		public string FullName { get; }

		public User(long id, string name, string password, string fullName)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (fullName == null) throw new ArgumentNullException(nameof(fullName));

			Id = id;
			Name = name;
			Password = password;
			FullName = fullName;
		}
	}
}