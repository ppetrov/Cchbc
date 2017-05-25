using System;
using System.Threading.Tasks;
using Cchbc.Features;
using iFSA.Common.Objects;

namespace iFSA
{
	public interface IDateTimeSelector
	{
		Task<DateTime?> ShowAsync(Feature feature, string title = "", DateTime? initialDateTime = null);
	}

	public interface IActivityCancelReasonSelector
	{
		Task<ActivityCloseReason> ShowAsync(Feature feature, ActivityType activityType);
	}
}