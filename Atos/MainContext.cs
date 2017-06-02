using System;
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
		public IModalDialog ModalDialog { get; }
		// For the cache of data
		public DataCache DataCache { get; } = new DataCache();
		// For feature tracking & timings
		public IFeatureManager FeatureManager { get; }
		// For localization
		public ILocalizationManager LocalizationManager { get; }

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
	}
}