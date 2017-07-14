using System;

namespace ConsoleClient.OrderModule.Models
{
	public sealed class OrderHeader
	{
		public long Id { get; set; }
		public DateTime DeliveryDate { get; set; } = DateTime.Today.AddDays(1);
		public string DeliveryAddress { get; set; } = string.Empty;
	}
}