using System;
using System.Windows.Input;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Logs;

namespace Cchbc
{
	public sealed class Core
	{
		public Action<string, LogLevel> Log { get; }
		public Func<ITransactionContext> ContextCreator { get; }
		public IModalDialog ModalDialog { get; }
		public DataCache DataCache { get; } = new DataCache();
		public FeatureManager FeatureManager { get; } = new FeatureManager();
		public LocalizationManager LocalizationManager { get; } = new LocalizationManager();

		public Core(Action<string, LogLevel> log, Func<ITransactionContext> contextCreator, IModalDialog modalDialog)
		{
			if (log == null) throw new ArgumentNullException(nameof(log));
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));

			this.Log = log;
			this.ContextCreator = contextCreator;
			this.ModalDialog = modalDialog;
		}
	}
}