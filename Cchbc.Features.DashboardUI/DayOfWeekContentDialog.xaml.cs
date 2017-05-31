using System;
using Windows.UI.Xaml.Controls;
using Cchbc.Validation;

namespace Cchbc.Features.DashboardUI
{
	public sealed partial class DayOfWeekContentDialog
	{
		private MainContext MainContext { get; }
		private Func<DayOfWeek, PermissionResult> Validator { get; }
		private Action<DayOfWeek> Operation { get; }

		public DayOfWeekContentDialog(MainContext mainContext, Func<DayOfWeek, PermissionResult> validator, Action<DayOfWeek> operation)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (validator == null) throw new ArgumentNullException(nameof(validator));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			this.InitializeComponent();

			MainContext = mainContext;
			this.Validator = validator;
			this.Operation = operation;
		}

		private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			var day = DayOfWeek.Sunday;

			var result = this.Validator(day);

			var canContinue = await this.MainContext.CanContinueAsync(result);

			args.Cancel = !canContinue;

			if (canContinue)
			{
				this.Operation(day);
			}
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}
	}
}
