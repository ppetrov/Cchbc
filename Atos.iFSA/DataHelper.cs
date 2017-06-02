using System;
using System.Collections.Generic;
using Atos;
using Atos.Client;
using Atos.Client.Data;
using iFSA.Common.Objects;
using iFSA.MasterDataModule.Objects;

namespace iFSA
{
	public static class DataHelper
	{
		private static readonly int OpenActivityStatus = 0;
		private static readonly int WorkingActivityStatus = 1;
		private static readonly int CancelActivityStatus = 2;
		private static readonly int CloseActivityStatus = 3;

		public static ActivityStatus GetOpenActivityStatus(MainContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetActivityStatus(context, OpenActivityStatus);
		}

		public static ActivityStatus GetWorkingActivityStatus(MainContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetActivityStatus(context, WorkingActivityStatus);
		}

		public static ActivityStatus GetCancelActivityStatus(MainContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetActivityStatus(context, CancelActivityStatus);
		}

		public static ActivityStatus GetCloseActivityStatus(MainContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetActivityStatus(context, CloseActivityStatus);
		}

		private static ActivityStatus GetActivityStatus(MainContext context, int statusId)
		{
			ActivityStatus activityStatus;

			using (var dbContext = context.DbContextCreator())
			{
				context.DataCache.GetValues<ActivityStatus>(dbContext).TryGetValue(statusId, out activityStatus);
				dbContext.Complete();
			}

			return activityStatus;
		}

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