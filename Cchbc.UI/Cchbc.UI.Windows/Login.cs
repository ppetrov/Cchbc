using System;
using Cchbc.Objects;

namespace Cchbc.UI
{
	public sealed class Login : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }
		public string Password { get; }
		public DateTime CreatedAt { get; }
		public bool IsSystem { get; set; }

		public Login(long id, string name, string password, DateTime createdAt, bool isSystem)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (password == null) throw new ArgumentNullException(nameof(password));

			this.Id = id;
			this.Name = name;
			this.Password = password;
			this.CreatedAt = createdAt;
			this.IsSystem = isSystem;
		}
	}
}