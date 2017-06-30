using System;
using System.Threading.Tasks;
using Atos.Client.Validation;

namespace Atos.Client.Selectors
{
	public interface ITimeSelector
	{
		Task ShowAsync(Func<DateTime, PermissionResult> validator, Action<DateTime> operation);
	}
}