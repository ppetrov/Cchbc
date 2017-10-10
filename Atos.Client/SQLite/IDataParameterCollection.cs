#if SQLITE
using System.Collections.Generic;

namespace System.SQLite
{
	public interface IDataParameterCollection : IList<IDbDataParameter>
	{

	}
}
#endif