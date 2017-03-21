using System;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Logs;

namespace Cchbc
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
	}
}