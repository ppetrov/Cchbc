﻿using System.Collections.Generic;

namespace Atos.iFSA.ArchitectureModule
{
	public interface IOrderScreenModifiableDataProvider
	{
		IEnumerable<OrderHeader> GetOrderHeaders();
		void Insert(OrderHeader model);
		void Delete(OrderHeader model);
	}
}