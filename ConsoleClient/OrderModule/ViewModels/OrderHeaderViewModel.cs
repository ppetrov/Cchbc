using System;
using Atos.Client;
using ConsoleClient.OrderModule.Models;

namespace ConsoleClient.OrderModule.ViewModels
{
	public sealed class OrderHeaderViewModel : ViewModel
	{
		public OrderHeader Header { get; }

		private DateTime _deliveryDate;
		public DateTime DeliveryDate
		{
			get { return _deliveryDate; }
			set
			{
				this.SetProperty(ref _deliveryDate, value);
				this.Header.DeliveryDate = value;
			}
		}

		private string _deliveryAddress;
		public string DeliveryAddress
		{
			get { return _deliveryAddress; }
			set
			{
				this.SetProperty(ref _deliveryAddress, value);
				this.Header.DeliveryAddress = value;
			}
		}

		public OrderHeaderViewModel(OrderHeader header)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));

			this.Header = header;
		}
	}
}