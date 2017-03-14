using System.Collections.Generic;
using Cchbc;
using Cchbc.Data;
using iFSA.Common.Objects;

namespace iFSA.Common.Data
{
	public static class DataProvider
	{
		public static Dictionary<long, List<ActivityCloseReason>> GetActivityCloseReasons(IDbContext context, DataCache cache)
		{
			var closeReasons = new Dictionary<long, List<ActivityCloseReason>>();

			context.Fill(closeReasons, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				var typeId = r.GetInt64(2);

				List<ActivityCloseReason> local;
				if (!map.TryGetValue(typeId, out local))
				{
					local = new List<ActivityCloseReason>();
					map.Add(typeId, local);
				}
				local.Add(new ActivityCloseReason(id, name));
			}, new Query(@"SELECT ID, NAME, ACTIVITY_TYPE_ID FROM ACTIVITY_CLOSE_REASONS"));

			return closeReasons;
		}

		public static Dictionary<long, List<ActivityCancelReason>> GetActivityCancelReasons(IDbContext context, DataCache cache)
		{
			var cancelReasons = new Dictionary<long, List<ActivityCancelReason>>();

			context.Fill(cancelReasons, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				var typeId = r.GetInt64(2);

				List<ActivityCancelReason> local;
				if (!map.TryGetValue(typeId, out local))
				{
					local = new List<ActivityCancelReason>();
					map.Add(typeId, local);
				}
				local.Add(new ActivityCancelReason(id, name));
			}, new Query(@"SELECT ID, NAME, ACTIVITY_TYPE_ID FROM ACTIVITY_CANCEL_REASONS"));

			return cancelReasons;
		}

		public static Dictionary<long, ActivityType> GetActivityTypes(IDbContext context, DataCache cache)
		{
			var categories = cache.GetValues<ActivityTypeCategory>(context);
			var byTypeCloseReasons = cache.GetValues<List<ActivityCloseReason>>(context);
			var byTypeCancelReasons = cache.GetValues<List<ActivityCancelReason>>(context);

			var types = new Dictionary<long, ActivityType>();

			context.Fill(types, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				var categoryId = r.GetInt64(2);

				var type = new ActivityType(id, name);
				var category = categories[categoryId];

				// Set Auto Selected ActivityType to the current if it's matching
				if (category.AutoSelectedActivityType != null && category.AutoSelectedActivityType.Id == id)
				{
					category.AutoSelectedActivityType = type;
				}

				// Add to related category
				category.Types.Add(type);

				List<ActivityCloseReason> closeReasonsByType;
				if (byTypeCloseReasons.TryGetValue(id, out closeReasonsByType))
				{
					type.CloseReasons.AddRange(closeReasonsByType);
				}

				List<ActivityCancelReason> cancelReasonsByType;
				if (byTypeCancelReasons.TryGetValue(id, out cancelReasonsByType))
				{
					type.CancelReasons.AddRange(cancelReasonsByType);
				}

				map.Add(id, type);
			}, new Query(@"SELECT ID, NAME, ACTIVITY_TYPE_CATEGORY_ID FROM ACTIVITY_TYPES"));

			// We don't need this rows anymore
			cache.RemoveValues<List<ActivityCloseReason>>();
			cache.RemoveValues<List<ActivityCancelReason>>();

			return types;
		}

		public static Dictionary<long, ActivityStatus> GetActivityStatuses(IDbContext context, DataCache cache)
		{
			var statuses = new Dictionary<long, ActivityStatus>();

			context.Fill(statuses, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);

				map.Add(id, new ActivityStatus(id, name));
			}, new Query(@"SELECT ID, NAME FROM ACTIVITY_STATUSES"));

			return statuses;
		}

		public static Dictionary<long, ActivityTypeCategory> GetActivityTypeCategories(IDbContext context, DataCache cache)
		{
			var categories = new Dictionary<long, ActivityTypeCategory>();

			context.Fill(categories, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);

				var category = new ActivityTypeCategory(id, name);
				if (!r.IsDbNull(2))
				{
					category.AutoSelectedActivityType = new ActivityType(r.GetInt64(2), string.Empty);
				}

				map.Add(id, category);
			}, new Query(@"SELECT ID, NAME, AUTO_SELECTED_ACTIVITY_TYPE_ID FROM ACTIVITY_TYPE_CATEGORIES"));

			return categories;
		}

		public static Dictionary<long, Outlet> GetOutlets(IDbContext context, DataCache cache)
		{
			var outlets = new Dictionary<long, Outlet>();

			context.Fill(outlets, (r, map) =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);

				var outlet = new Outlet(id, name);

				map.Add(id, outlet);
			}, new Query(@"SELECT ID, NAME, AUTO_SELECTED_ACTIVITY_TYPE_ID FROM ACTIVITY_TYPE_CATEGORIES"));


			var addresses = cache.GetValues<List<OutletAddress>>(context);
			foreach (var outlet in outlets.Values)
			{
				List<OutletAddress> byOutlet;
				if (addresses.TryGetValue(outlet.Id, out byOutlet))
				{
					outlet.Addresses.AddRange(byOutlet);
				}
			}

			// We don't need them anymore as they are assigned to the respective outlets
			cache.RemoveValues<List<OutletAddress>>();



			return outlets;
		}

		public static Dictionary<long, List<OutletAddress>> GetOutletAddressed(IDbContext context, DataCache cache)
		{
			var addresses = new Dictionary<long, List<OutletAddress>>();

			context.Fill(addresses, (r, map) =>
			{
				List<OutletAddress> byOutlet;
				if (!map.TryGetValue(-1, out byOutlet))
				{
					byOutlet = new List<OutletAddress>();
					map.Add(-1, byOutlet);
				}

				byOutlet.Add(new OutletAddress(-1, 0, string.Empty, -1, string.Empty));
			}, new Query(@"SELECT ID, NAME, AUTO_SELECTED_ACTIVITY_TYPE_ID FROM ACTIVITY_TYPE_CATEGORIES"));

			return addresses;
		}
	}
}