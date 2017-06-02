using System;
using System.Threading.Tasks;
using Atos.Client.Validation;

namespace iFSA
{
	public interface IDateTimeSelector
	{
		Task ShowAsync(Func<DateTime, PermissionResult> validator, Action<DateTime> operation);
	}
}