using System.Threading.Tasks;
using Cchbc.Features;

namespace Cchbc.Dialog
{
	public interface IModalDialog
	{
		Task<DialogResult> ShowAsync(string message, Feature feature, DialogType? type = null);
	}
}