using System;
using System.Threading.Tasks;
using Cchbc.Features;

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
			this.AcceptAction =
				this.DeclineAction =
					this.CancelAction = EmptyAction;
		}

		public Task DisplayAsync(string message, Feature feature)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.UpdateActions(feature);
			return this.ShowAsync(message, DialogType.Message);
		}

		public Task ConfirmAsync(string message, Feature feature, DialogType ? type = null)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.UpdateActions(feature);
			return this.ShowAsync(message, type ?? DialogType.AcceptDecline);
		}

		private void UpdateActions(Feature feature)
		{
			if (feature != null)
			{
				feature.Pause();
				var a = this.AcceptAction;
				this.AcceptAction = () =>
				{
					feature.Resume();
					a();
				};
				var d = this.DeclineAction;
				this.DeclineAction = () =>
				{
					feature.Resume();
					d();
				};
				var c = this.CancelAction;
				this.CancelAction = () =>
				{
					feature.Resume();
					c();
				};
			}
		}

		public abstract Task ShowAsync(string message, DialogType? type = null);
	}
}