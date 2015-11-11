using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Localization;

namespace Cchbc.App
{
	public sealed class Core
	{
		public static Core Current { get; set; } = new Core();

		public ILogger Logger { get; set; }
		public QueryHelper QueryHelper { get; set; }
		public DataCache DataCache { get; set; }
		public FeatureManager FeatureManager { get; set; }
		public LocalizationManager LocalizationManager { get; set; }
	}
}