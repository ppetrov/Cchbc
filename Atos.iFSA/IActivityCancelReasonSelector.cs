using System;
using System.Threading.Tasks;
using Atos.Client.Validation;
using Atos.iFSA.Objects;

namespace Atos.iFSA
{
	public interface IActivityCancelReasonSelector
	{
		Task ShowAsync(Activity activity, Func<ActivityCancelReason, PermissionResult> validator, Action<ActivityCancelReason> operation);
	}
}