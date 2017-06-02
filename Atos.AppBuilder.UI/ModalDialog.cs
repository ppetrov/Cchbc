using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Atos.Client.Dialog;
using Atos.Client.Validation;

namespace Atos.AppBuilder.UI
{
	public sealed class ModalDialog : IModalDialog
	{
		public async Task<DialogResult> ShowAsync(PermissionResult message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			var dialog = new MessageDialog(message.LocalizationKeyName);

			UICommandInvokedHandler empty = cmd => { };
			dialog.Commands.Add(new UICommand(@"Accept", empty, DialogResult.Accept));
			dialog.Commands.Add(new UICommand(@"Cancel", empty, DialogResult.Cancel));
			dialog.Commands.Add(new UICommand(@"Decline", empty, DialogResult.Decline));

			var task = dialog.ShowAsync().AsTask();
			var result = await task;

			return (DialogResult)result.Id;
		}
	}
}