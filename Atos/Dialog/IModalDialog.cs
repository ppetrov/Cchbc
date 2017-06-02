using System.Threading.Tasks;
using Atos.Client.Validation;

namespace Atos.Client.Dialog
{
	public interface IModalDialog
	{
		Task<DialogResult> ShowAsync(PermissionResult message);
	}
}