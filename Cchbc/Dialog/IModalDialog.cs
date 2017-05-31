using System.Threading.Tasks;
using Cchbc.Validation;

namespace Cchbc.Dialog
{
	public interface IModalDialog
	{
		Task<DialogResult> ShowAsync(PermissionResult message);
	}
}