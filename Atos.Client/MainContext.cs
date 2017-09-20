using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atos.Client.Data;
using Atos.Client.Dialog;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;
using Atos.Client.Validation;

namespace Atos.Client
{
	public sealed class MainContext
	{
		// For logging
		public Action<string, LogLevel> Log { get; }
		// For Query the db
		public Func<IDbContext> DbContextCreator { get; }
		// For displaying modal dialogs
		private IModalDialog ModalDialog { get; }
		// For the cache of data
		public DataCache DataCache { get; } = new DataCache();
		// For feature tracking & timings
		private IFeatureManager FeatureManager { get; }
		// For localization
		private ILocalizationManager LocalizationManager { get; }

		public IServiceLocator ServiceLocator { get; }

		public MainContext(Action<string, LogLevel> log, Func<IDbContext> dbContextCreator, IModalDialog modalDialog, IFeatureManager featureManager, ILocalizationManager localizationManager)
		{
			if (log == null) throw new ArgumentNullException(nameof(log));
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));
			if (featureManager == null) throw new ArgumentNullException(nameof(featureManager));

			this.Log = log;
			this.DbContextCreator = dbContextCreator;
			this.ModalDialog = modalDialog;
			this.FeatureManager = featureManager;
			this.LocalizationManager = localizationManager;
		}

		public void LoadLocalization(IEnumerable<string> lines)
		{
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			this.LocalizationManager.Load(lines);
		}

		public void Save(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.FeatureManager.Save(feature, details);
		}

		public void Save(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Log(exception.ToString(), LogLevel.Error);
			this.FeatureManager.Save(feature, exception);
		}

		public FeatureContext CreateFeatureContext()
		{
			return new FeatureContext(this);
		}

		public async Task<bool> CanContinueAsync(PermissionResult permissionResult)
		{
			if (permissionResult == null) throw new ArgumentNullException(nameof(permissionResult));

			switch (permissionResult.Type)
			{
				case PermissionType.Allow:
					return true;
				case PermissionType.Confirm:
					var confirmation = await this.ModalDialog.ShowAsync(permissionResult);
					if (confirmation == DialogResult.Accept)
					{
						return true;
					}
					break;
				case PermissionType.Deny:
					await this.ModalDialog.ShowAsync(permissionResult);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return false;
		}

		public string GetLocalized(LocalizationKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			return this.LocalizationManager.Get(key);
		}

		public Task ShowMessageAsync(LocalizationKey localizationKey)
		{
			if (localizationKey == null) throw new ArgumentNullException(nameof(localizationKey));

			return this.ModalDialog.ShowAsync(PermissionResult.Deny(this.GetLocalized(localizationKey)));
		}

		public T GetService<T>()
		{
			throw new NotImplementedException();
		}
	}
}