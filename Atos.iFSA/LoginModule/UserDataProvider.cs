using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.LoginModule
{
	public interface IUserDataProvider
	{
		User[] GetUsers(DataQueryContext context);
	}
}
