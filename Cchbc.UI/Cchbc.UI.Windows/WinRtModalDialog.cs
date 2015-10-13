using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Cchbc.Dialog;

namespace Cchbc.UI.Comments
{
	public sealed class WinRtModalDialog : ModalDialog
	{
		private MessageDialog _dialog;

		public override async Task ShowAsync(string message, DialogType? type = null)
		{
			_dialog = new MessageDialog(message);

			switch (type)
			{
				case DialogType.Message:
				case null:
					_dialog.Commands.Add(new UICommand("Close", _ => { this.CancelAction(); }));
					break;
				case DialogType.AcceptDecline:
					_dialog.Commands.Add(new UICommand("Yes", _ => { this.AcceptAction(); }));
					_dialog.Commands.Add(new UICommand("No", _ => { this.DeclineAction(); }));
					break;
				case DialogType.AcceptDeclineCancel:
					_dialog.Commands.Add(new UICommand("Yes", _ => { this.AcceptAction(); }));
					_dialog.Commands.Add(new UICommand("No", _ => { this.DeclineAction(); }));
					_dialog.Commands.Add(new UICommand("Cancel", _ => { this.CancelAction(); }));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			await _dialog.ShowAsync();
		}
	}
}