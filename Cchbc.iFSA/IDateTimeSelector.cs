using System;
using System.Threading.Tasks;
using Cchbc.Features;
using Cchbc.Validation;
using iFSA.Common.Objects;

namespace iFSA
{
	public interface ITimeSelector
	{
		Func<DateTime, PermissionResult> TimeValidator { get; set; }
		Action<DateTime> Callback { get; set; }
		void SelectTime(Feature feature, string title = "", DateTime? initialDateTime = null);
	}

	public interface IActivityCancelReasonSelector
	{
		Task<ActivityCloseReason> ShowAsync(Feature feature, ActivityType activityType);
	}
}