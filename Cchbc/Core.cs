using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Helpers;
using Cchbc.Localization;

namespace Cchbc
{
	public sealed class Core
	{
		public IModalDialog ModalDialog { get; set; }
		public QueryHelper QueryHelper { get; } = new QueryHelper();
		public DataCache DataCache { get; } = new DataCache();
		public FeatureManager FeatureManager { get; } = new FeatureManager();
		public LocalizationManager LocalizationManager { get; } = new LocalizationManager();

		public Helper<T> GetHelper<T>()
		{
			return this.DataCache.Get<T>();
		}
	}
}