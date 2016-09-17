using System;

namespace Cchbc.Features.ExceptionsModule
{
	public sealed class ExceptionsCount
	{
		public DateTime DateTime { get; }
		public int Count { get; }

		public ExceptionsCount(DateTime dateTime, int count)
		{
			this.DateTime = dateTime;
			this.Count = count;
		}
	}
}