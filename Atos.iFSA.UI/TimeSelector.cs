using System;
using System.Threading.Tasks;
using Atos.Client;
using Atos.Client.Localization;
using Atos.Client.Selectors;
using Atos.Client.Validation;

namespace Atos.iFSA.UI
{
	public sealed class TimeSelector : ITimeSelector
	{
		public MainContext MainContext { get; }

		private Func<DateTime, PermissionResult> Validator { get; set; }
		private Action<DateTime> Operation { get; set; }

		public TimeSelector(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public Task ShowAsync(Func<DateTime, PermissionResult> validator, Action<DateTime> operation)
		{
			if (validator == null) throw new ArgumentNullException(nameof(validator));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			this.Validator = validator;
			this.Operation = operation;

			//TODO : Display UI Control
			throw new NotImplementedException();
		}

		public void Ok()
		{
			var selectedDate = DateTime.Today;

			var permissionResult = this.Validator(selectedDate);
			if (permissionResult == PermissionResult.Allow)
			{
				this.Operation(selectedDate);
				this.Cancel();
			}
			else
			{
				this.MainContext.ShowMessageAsync(new LocalizationKey(string.Empty, permissionResult.LocalizationKeyName));
			}
		}

		public void Cancel()
		{
			// TODO : Close UI control
		}
	}
}