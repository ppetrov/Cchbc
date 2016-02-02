﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.App.AgendaModule;
using Cchbc.Dialog;
using Cchbc.Features;


namespace Cchbc.AppBuilder.UI
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			try
			{
				var viewModel = new AgendaViewModel(new AgendaManager(new AgendaSettings(), new ModalDialog()), new FeatureManager());
				await viewModel.DeleteAsync(new ActivityViewModel(new Activity()));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}
	}

	public sealed class ModalDialog : IModalDialog
	{
		public async Task<DialogResult> ShowAsync(string message, Feature feature, DialogType? type = null)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var dialog = new MessageDialog(message);

			UICommandInvokedHandler empty = cmd => { };
			dialog.Commands.Add(new UICommand(@"Accept", empty, DialogResult.Accept));
			dialog.Commands.Add(new UICommand(@"Cancel", empty, DialogResult.Cancel));
			dialog.Commands.Add(new UICommand(@"Decline", empty, DialogResult.Decline));

			feature.Pause();
			var task = dialog.ShowAsync().AsTask();
			feature.Resume();

			var result = await task;
			return (DialogResult)result.Id;
		}
	}
}
