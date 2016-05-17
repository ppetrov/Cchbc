using System;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class DashboardExceptionViewModel : ViewModel
	{
		public DateTime DateTime { get; }
		public int Count { get; }

		public DashboardExceptionViewModel(DashboardException exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.DateTime = exception.DateTime;
			this.Count = exception.Count;
		}
	}
}