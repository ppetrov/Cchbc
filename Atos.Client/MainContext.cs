using System;
using System.Threading.Tasks;
using Atos.Client.Dialog;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.Client.Validation;

namespace Atos.Client
{
	public sealed class MainContext
	{
		public DataCache DataCache { get; } = new DataCache();
		public ServiceLocator ServiceLocator { get; } = new ServiceLocator();

		public T GetService<T>()
		{
			return this.ServiceLocator.GetService<T>();
		}

		public void RegisterService<T>(T service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			this.ServiceLocator.RegisterService(service);
		}

		public DataQueryContext CreateDataQueryContext()
		{
			return new DataQueryContext(this);
		}

		public string GetLocalized(LocalizationKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			return this.GetService<LocalizationManager>().Get(key);
		}

		public void Log(Exception ex)
		{
			if (ex == null) throw new ArgumentNullException(nameof(ex));

			this.Log(ex.ToString(), LogLevel.Error);
		}

		public void Log(string message, LogLevel logLevel)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			this.GetService<ILogger>().Log(message, logLevel);
		}

		public void Save(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.GetService<IFeatureManager>().Save(feature, details);
		}

		public void Save(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Log(feature.Context + @"->" + feature.Name + Environment.NewLine + exception, LogLevel.Error);
			this.GetService<IFeatureManager>().Save(feature, exception);
		}






		//
		// TODO : Review these methods
		//

		public async Task<bool> CanContinueAsync(PermissionResult permissionResult)
		{
			if (permissionResult == null) throw new ArgumentNullException(nameof(permissionResult));

			var modalDialog = this.GetService<IModalDialog>();
			switch (permissionResult.Type)
			{
				case PermissionType.Allow:
					return true;
				case PermissionType.Confirm:
					var confirmation = await modalDialog.ShowAsync(permissionResult);
					if (confirmation == DialogResult.Accept)
					{
						return true;
					}
					break;
				case PermissionType.Deny:
					await modalDialog.ShowAsync(permissionResult);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return false;
		}

		public Task ShowMessageAsync(LocalizationKey localizationKey)
		{
			if (localizationKey == null) throw new ArgumentNullException(nameof(localizationKey));

			return this.GetService<IModalDialog>().ShowAsync(PermissionResult.Deny(this.GetLocalized(localizationKey)));
		}
	}
}