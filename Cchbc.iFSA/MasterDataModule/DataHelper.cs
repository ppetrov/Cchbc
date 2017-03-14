using System;
using Cchbc;
using Cchbc.Data;
using iFSA.Common.Objects;
using iFSA.MasterDataModule.Objects;

namespace iFSA.MasterDataModule
{
	public static class DataHelper
	{
		public static TradeChannel GetTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			TradeChannel result;
			cache.GetValues<TradeChannel>(context).TryGetValue(-1, out result);

			return result ?? TradeChannel.Empty;
		}

		public static SubTradeChannel GetSubTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			SubTradeChannel result;
			cache.GetValues<SubTradeChannel>(context).TryGetValue(-1, out result);

			return result ?? SubTradeChannel.Empty;
		}
	}
}