using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Atos.Client.Dialog;
using Atos.Client.Validation;

namespace Atos.iFSA.UI
{
	public sealed class ModalDialog : IModalDialog
	{
		// TODO : Use custom Content Dialog to allow styling of the application
		public async Task<DialogResult> ShowAsync(PermissionResult message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			var dialog = new MessageDialog(message.LocalizationKeyName);

			UICommandInvokedHandler empty = cmd => { };

			switch (message.Type)
			{
				case PermissionType.Allow:
					break;
				case PermissionType.Confirm:
					dialog.Commands.Add(new UICommand(@"Accept", empty, DialogResult.Accept));
					dialog.Commands.Add(new UICommand(@"Decline", empty, DialogResult.Decline));
					dialog.Commands.Add(new UICommand(@"Cancel", empty, DialogResult.Cancel));
					break;
				case PermissionType.Deny:
					dialog.Commands.Add(new UICommand(@"OK", empty, DialogResult.Cancel));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var task = dialog.ShowAsync().AsTask();
			var result = await task;

			return (DialogResult)result.Id;
		}
	}
}