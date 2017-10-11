using System;
using System.Threading.Tasks;
using Atos.Client.Validation;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule
{
	public interface IActivityCancelReasonSelector
	{
		Task ShowAsync(Activity activity, Func<ActivityCancelReason, PermissionResult> validator, Action<ActivityCancelReason> operation);
	}
}