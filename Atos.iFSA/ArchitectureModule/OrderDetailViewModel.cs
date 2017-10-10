using System;
using Atos.Client;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class OrderDetailViewModel : ViewModel
	{
		public OrderDetail OrderDetail { get; }
		public string Name => this.OrderDetail.Name;

		public OrderDetailViewModel(OrderDetail orderDetail)
		{
			if (orderDetail == null) throw new ArgumentNullException(nameof(orderDetail));

			this.OrderDetail = orderDetail;
		}
	}
}