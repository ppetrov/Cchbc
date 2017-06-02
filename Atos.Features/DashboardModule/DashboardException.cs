using System;

namespace Atos.Features.DashboardModule
{
	public sealed class DashboardException
	{
		public DateTime DateTime { get; }
		public int Count { get; }

		public DashboardException(DateTime dateTime, int count)
		{
			this.DateTime = dateTime;
			this.Count = count;
		}
	}
}