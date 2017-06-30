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

		private Func<DateTime, PermissionResult> _validator;
		private Action<DateTime> _operation;

		public TimeSelector(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public Task ShowAsync(Func<DateTime, PermissionResult> validator, Action<DateTime> operation)
		{
			if (validator == null) throw new ArgumentNullException(nameof(validator));
			if (operation == null) throw new ArgumentNullException(nameof(operation));

			_validator = validator;
			_operation = operation;

			//TODO : Display UI Control
			throw new NotImplementedException();
		}

		public void Ok()
		{
			var selectedDate = DateTime.Today;

			var permissionResult = _validator(selectedDate);
			if (permissionResult == PermissionResult.Allow)
			{
				_operation(selectedDate);
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