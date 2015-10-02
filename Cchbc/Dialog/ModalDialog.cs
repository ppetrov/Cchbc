using System;
using System.Threading.Tasks;

namespace Cchbc.Dialog
{
	public abstract class ModalDialog
	{
		private static readonly Action EmptyAction = () => { };

		public Action AcceptAction { get; set; }
		public Action DeclineAction { get; set; }
		public Action CancelAction { get; set; }

		protected ModalDialog()
		{
			this.AcceptAction = EmptyAction;
			this.DeclineAction = EmptyAction;
			this.CancelAction = EmptyAction;
		}

		public Task DisplayAsync(string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return this.ShowAsync(message, DialogType.Message);
		}

		public Task ConfirmAsync(string message, DialogType? type = null)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return this.ShowAsync(message, type ?? DialogType.AcceptDecline);
		}

		public abstract Task ShowAsync(string message, DialogType? type = null);
	}
}