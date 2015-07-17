using System;
using System.Threading.Tasks;

namespace Cchbc.Dialog
{
	public abstract class DialogBase
	{
		public Action AcceptAction { get; set; }
		public Action DeclineAction { get; set; }
		public Action CancelAction { get; set; }

		private readonly Action _emptyAction = () => { };

		protected DialogBase()
		{
			this.AcceptAction = _emptyAction;
			this.DeclineAction = _emptyAction;
			this.CancelAction = _emptyAction;
		}

		public Task DisplayAsync(string message)
		{
			if (message == null) throw new ArgumentNullException("message");

			return this.ShowAsync(message, DialogType.None);
		}

		public Task ConfirmAsync(string message, DialogType? type = null)
		{
			if (message == null) throw new ArgumentNullException("message");

			return this.ShowAsync(message, type ?? DialogType.YesNo);
		}

		public abstract Task ShowAsync(string message, DialogType? type = null);
	}
}