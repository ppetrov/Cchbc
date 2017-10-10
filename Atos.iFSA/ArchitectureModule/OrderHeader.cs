using System;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class OrderHeader
	{
		public long Id { get; }
		public string Name { get; }
		public DateTime CreatedAt { get; }
		public OrderDetail[] OrderDetails { get; }
	}
}