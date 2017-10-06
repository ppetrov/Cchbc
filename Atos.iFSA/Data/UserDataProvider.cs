using System;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.Data
{
	public interface IUserDataProvider
	{
		User[] GetUsers(DataQueryContext context);
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
