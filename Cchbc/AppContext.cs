using System;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Logs;

namespace Cchbc
{
	public sealed class AppContext
	{
		public Action<string, LogLevel> Log { get; }
		public Func<IDbContext> DbContextCreator { get; }
		public IModalDialog ModalDialog { get; }
		public DataCache DataCache { get; } = new DataCache();
		public FeatureManager FeatureManager { get; } = new FeatureManager();
		public LocalizationManager LocalizationManager { get; } = new LocalizationManager();

		public AppContext(Action<string, LogLevel> log, Func<IDbContext> dbContextCreator, IModalDialog modalDialog)
		{
			if (log == null) throw new ArgumentNullException(nameof(log));
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));

			this.Log = log;
			this.DbContextCreator = dbContextCreator;
			this.ModalDialog = modalDialog;
		}
	}
}