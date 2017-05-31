using System;
using System.Threading.Tasks;
using Cchbc.Validation;
using iFSA.Common.Objects;

namespace iFSA
{
	public interface IActivityCancelReasonSelector
	{
		Task ShowAsync(Activity activity, Func<ActivityCancelReason, PermissionResult> validator, Action<ActivityCancelReason> operation);
	}
}