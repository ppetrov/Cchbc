using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Helpers;
using Cchbc.Localization;

namespace Cchbc
{
	public sealed class Core
	{
		public QueryExecutor QueryExecutor { get; set; }
		public DataCache DataCache { get; set; }
		public FeatureManager FeatureManager { get; set; }
		public LocalizationManager LocalizationManager { get; set; }

		public Helper<T> GetHelper<T>()
		{
			return this.DataCache.GetHelper<T>();
		}
	}
}