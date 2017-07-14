using System;
using System.Collections.Generic;

namespace ConsoleClient.OrderModule.Models
{
	public sealed class Order
	{
		public OrderHeader Header { get; }
		public List<OrderDetail> Details { get; } = new List<OrderDetail>();

		public Order(OrderHeader header)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));

			this.Header = header;
		}
	}
}