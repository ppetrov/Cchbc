using System;
using System.Collections.Generic;
using System.Linq;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.Data
{
	public interface IUserDataProvider
	{
		User[] GetUsers(IDbContext context);
	}

	public static class UserDataProvider
	{
		public static User[] GetUsers(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return null;
		}
	}
}
