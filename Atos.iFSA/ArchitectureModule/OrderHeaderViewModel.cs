using System;
using Atos.Client;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class OrderHeaderViewModel : ViewModel
	{
		public OrderHeader OrderHeader { get; }
		public string Name => this.OrderHeader.Name;
		public string CreatedAt => this.OrderHeader.CreatedAt.ToString(@"O");
		public OrderDetailViewModel[] Details { get; }

		public OrderHeaderViewModel(OrderHeader orderHeader)
		{
			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));
			OrderHeader = orderHeader;

			this.Details = new OrderDetailViewModel[orderHeader.OrderDetails.Length];
			for (var i = 0; i < orderHeader.OrderDetails.Length; i++)
			{
				this.Details[i] = new OrderDetailViewModel(orderHeader.OrderDetails[i]);
			}
		}
	}
}