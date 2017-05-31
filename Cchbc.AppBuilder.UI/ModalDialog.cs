﻿using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Validation;

namespace Cchbc.AppBuilder.UI
{
	public sealed class ModalDialog : IModalDialog
	{
		public async Task<DialogResult> ShowAsync(PermissionResult message, Feature feature)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

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