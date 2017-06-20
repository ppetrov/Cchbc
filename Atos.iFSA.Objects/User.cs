using System;
using System.Collections.Generic;

namespace Atos.iFSA.Objects
{
	public sealed class User
	{
		public long Id { get; }
		public string Name { get; }
		public string Address { get; }
		public string Email { get; }
		public string FullName { get; }
		public string Industry { get; }
		public bool IsAuditor { get; }
		public bool IsTeamLeader { get; }
		public List<User> Users { get; } = new List<User>();

		public User(long id, string name, string address, string email, string fullName, string industry, bool isAuditor, bool isTeamLeader)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (address == null) throw new ArgumentNullException(nameof(address));
			if (email == null) throw new ArgumentNullException(nameof(email));
			if (fullName == null) throw new ArgumentNullException(nameof(fullName));
			if (industry == null) throw new ArgumentNullException(nameof(industry));

			this.Id = id;
			this.Name = name;
			this.Address = address;
			this.Email = email;
			this.FullName = fullName;
			this.Industry = industry;
			this.IsAuditor = isAuditor;
			this.IsTeamLeader = isTeamLeader;
		}
	}
}