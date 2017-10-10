using System.Collections.Generic;

namespace Atos.iFSA.ArchitectureModule
{
	public interface IOrderScreenDataProvider
	{
		IEnumerable<OrderHeader> GetOrderHeaders();
	}
}