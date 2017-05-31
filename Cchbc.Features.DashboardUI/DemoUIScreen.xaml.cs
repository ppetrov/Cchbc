using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Cchbc.Dialog;
using Cchbc.Validation;

namespace Cchbc.Features.DashboardUI
{
	public sealed class ModalDialog : IModalDialog
	{
		public async Task<DialogResult> ShowAsync(PermissionResult message)
		{
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

	public interface IDayOfWeekSelector
	{
		Task ShowAsync(Func<DayOfWeek, PermissionResult> validator, Action<DayOfWeek> operation);
	}

	public sealed class DayOfWeekSelector : IDayOfWeekSelector
	{
		public MainContext MainContext { get; }

		public DayOfWeekSelector(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public Task ShowAsync(Func<DayOfWeek, PermissionResult> validator, Action<DayOfWeek> operation)
		{
			return new DayOfWeekContentDialog(this.MainContext, validator, operation).ShowAsync().AsTask();
		}
	}

	public sealed partial class DemoUIScreen
	{
		public MainContext MainContext { get; set; }

		public DemoUIScreen()
		{
			this.InitializeComponent();

			this.MainContext = new MainContext((m, l) =>
			{
				Debug.WriteLine(l + ":" + m);
			}, null, new ModalDialog(), null, null);
		}

		private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			var feature = new Feature(nameof(DemoUIScreen), nameof(UIElement_OnTapped));
			try
			{
				Debug.WriteLine(@"Save feature:" + feature.Context + ":" + feature.Name);

				var selector = new DayOfWeekSelector(this.MainContext);

				await selector.ShowAsync(
					day =>
					{
						if (day == DayOfWeek.Sunday)
						{
							return PermissionResult.Deny(@"CannotSelectSunday");
						}
						if (day == DayOfWeek.Saturday)
						{
							return PermissionResult.Confirm(@"SureToSelectSaturday");
						}
						return PermissionResult.Allow;
					},
					day =>
					{
						Debug.WriteLine(@"Change object property to:" + day);
					});
			}
			catch (Exception ex)
			{
				Debug.WriteLine(@"Save feature with exception:" + feature.Context + ":" + feature.Name + Environment.NewLine + ex);
			}
		}
	}
}
