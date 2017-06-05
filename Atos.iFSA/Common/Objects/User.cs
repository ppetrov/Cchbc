using System;

namespace Atos.iFSA.Common.Objects
{
	public sealed class User
	{
		public long Id { get; }
		public string Username { get; }

		public User(long id, string username)
		{
			if (username == null) throw new ArgumentNullException(nameof(username));

			this.Id = id;
			this.Username = username;
		}
	}
}