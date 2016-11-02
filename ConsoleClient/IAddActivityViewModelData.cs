using System.Collections.Generic;

namespace ConsoleClient
{
	public interface IAddActivityViewModelData
	{
		List<ActivityTypeCategory> Categories { get; }
		List<Outlet> Outlets { get; }
		bool WithVisit { get; }
	}
}