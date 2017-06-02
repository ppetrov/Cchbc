using System.Threading.Tasks;
using Atos.Validation;

namespace Atos.Dialog
{
	public interface IModalDialog
	{
		Task<DialogResult> ShowAsync(PermissionResult message);
	}
}