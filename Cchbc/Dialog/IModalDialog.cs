using System.Threading.Tasks;
using Cchbc.Features;
using Cchbc.Validation;

namespace Cchbc.Dialog
{
	public interface IModalDialog
	{
		Task<DialogResult> ShowAsync(string message, Feature feature, PermissionType? type = null);
	}
}