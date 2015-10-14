using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Helpers;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc
{
	public sealed class Core
	{
		public DataCache DataCache { get; } = new DataCache(new HelperCache(), new ManagerCache());

		public async Task InitApplication()
		{
			// TODO : Init logger !!!
			var logger = default(ILogger);

			// TODO : Init Db !!!

			await this.DataCache.LoadAsync(logger);
		}
	}

	public sealed class DataCache
	{
		public HelperCache HelperCache { get; }
		public ManagerCache ManagerCache { get; }

		public DataCache(HelperCache helperCache, ManagerCache managerCache)
		{
			if (helperCache == null) throw new ArgumentNullException(nameof(helperCache));
			if (managerCache == null) throw new ArgumentNullException(nameof(managerCache));

			this.HelperCache = helperCache;
			this.ManagerCache = managerCache;
		}

		public async Task LoadAsync(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			var s = Stopwatch.StartNew();
			try
			{
				await this.HelperCache.LoadAsync(logger);
			}
			finally
			{
				logger.Info($@"{nameof(HelperCache)}:{s.ElapsedMilliseconds}ms");
			}

			s.Restart();
			try
			{
				await this.ManagerCache.LoadAsync(this.HelperCache, logger);
			}
			finally
			{
				logger.Info($@"{nameof(ManagerCache)}:{s.ElapsedMilliseconds}ms");
			}
		}
	}

	public sealed class HelperCache
	{
		private readonly UserHelper _userHelper = new UserHelper();
		private readonly OutletHelper _outletHelper = new OutletHelper();
		private readonly OutletCommentTypeHelper _outletOutletCommentTypeHelper = new OutletCommentTypeHelper();
		private readonly ActivityTypeHelper _activityActivityTypeHelper = new ActivityTypeHelper();
		private readonly ActiviStatusHelper _activityStatusHelper = new ActiviStatusHelper();
		private readonly EquipmentTypeHelper _equipmentEquipmentTypeHelper = new EquipmentTypeHelper();

		public UserHelper UserHelper => _userHelper;
		public OutletHelper OutletHelper => _outletHelper;
		public OutletCommentTypeHelper OutletCommentTypeHelper => _outletOutletCommentTypeHelper;
		public ActivityTypeHelper ActivityTypeHelper => _activityActivityTypeHelper;
		public ActiviStatusHelper ActivityStatusHelper => _activityStatusHelper;
		public EquipmentTypeHelper EquipmentTypeHelper => _equipmentEquipmentTypeHelper;

		public async Task LoadAsync(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			await _userHelper.LoadAsync(new UserAdapter());

			var outletOutletAssignmentHelper = new OutletAssignmentHelper();
			await outletOutletAssignmentHelper.LoadAsync(new OutletAssignmentAdapter());

			// Load outlets
			await _outletHelper.LoadAsync(new OutletAdapter(outletOutletAssignmentHelper.Items));

			// Load outlet comment types
			await _outletOutletCommentTypeHelper.LoadAsync(new OutletCommentTypeAdapter());

			// Load activity types
			await _activityActivityTypeHelper.LoadAsync(new ActivityTypeAdapter());

			// Load activity statuses
			await _activityStatusHelper.LoadAsync(new ActiviStatusAdapter());

			// Load equipment types
			await _equipmentEquipmentTypeHelper.LoadAsync(new EquipmentTypeAdapter());
		}
	}

	public sealed class ManagerCache
	{
		private readonly OutletCommentManager _outletOutletCommentManager = new OutletCommentManager(null, null, null);

		public OutletCommentManager OutletCommentManager => _outletOutletCommentManager;

		public async Task LoadAsync(HelperCache cache, ILogger logger)
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			var helper = new OutletCommentHelper();
			await helper.LoadAsync(new OutletCommentAdapter(cache.OutletHelper.Items, cache.OutletCommentTypeHelper.Items, cache.UserHelper.Items));

			_outletOutletCommentManager.LoadData(helper.Items.Values.Select(v => new OutletCommentViewItem(v)));
		}
	}

	public sealed class EquipmentType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class EquipmentTypeAdapter : IReadOnlyAdapter<EquipmentType>
	{
		public Task PopulateAsync(Dictionary<long, EquipmentType> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class EquipmentTypeHelper : Helper<EquipmentType>
	{

	}

	public sealed class Equipment : IDbObject
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public EquipmentType Type { get; set; }
		public string Barcode { get; set; } = string.Empty;
		public string SerialNumber { get; set; } = string.Empty;
	}

	public sealed class EquipmentAdapter : IReadOnlyAdapter<Equipment>
	{
		private readonly Dictionary<long, Outlet> _outlets;

		public EquipmentAdapter(Dictionary<long, Outlet> outlets)
		{
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));

			_outlets = outlets;
		}

		public Task PopulateAsync(Dictionary<long, Equipment> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class EquipmentHelper : Helper<Equipment>
	{
		public List<Equipment> GetByOutlet(Outlet outlet)
		{
			var equipments = new List<Equipment>();

			foreach (var equipment in this.Items.Values)
			{
				if (equipment.Outlet == outlet)
				{
					equipments.Add(equipment);
				}
			}

			return equipments;
		}
	}

	public sealed class OutletCommentViewItem : ViewItem<OutletComment>
	{
		public OutletCommentViewItem(OutletComment item) : base(item)
		{
		}
	}

	public sealed class OutletCommentManager : Manager<OutletComment, OutletCommentViewItem>
	{
		public OutletCommentManager(IModifiableAdapter<OutletComment> adapter, Sorter<OutletCommentViewItem> sorter, Searcher<OutletCommentViewItem> searcher, FilterOption<OutletCommentViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		{
		}

		public override ValidationResult[] ValidateProperties(OutletCommentViewItem viewItem)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanAddAsync(OutletCommentViewItem viewItem)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(OutletCommentViewItem viewItem)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(OutletCommentViewItem viewItem)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AgendaModule
	{
		private readonly List<AgendaItem> _items = new List<AgendaItem>();

		public async Task LoadAsync(DataCache dataCache, DateTime date)
		{
			if (dataCache == null) throw new ArgumentNullException(nameof(dataCache));

			var helpers = dataCache.HelperCache;
			var managers = dataCache.ManagerCache;

			var outlets = helpers.OutletHelper.Items;

			// Remove visits for outlets without assingnments
			var visits = new List<Visit>();
			var visitAdapter = new VisitAdapter(outlets, helpers.ActivityTypeHelper.Items, helpers.ActivityStatusHelper.Items);
			foreach (var visit in await visitAdapter.GetByDateAsync(date))
			{
				var outlet = visit.Outlet;
				if (outlet.HasValidAssignment(date))
				{
					visits.Add(visit);
				}
			}

			// Load outlet comments
			var outletComments = managers.OutletCommentManager.ViewItems.Select(v => v.Item).ToList();

			// Load equipments
			var equipmentHelper = new EquipmentHelper();
			await equipmentHelper.LoadAsync(new EquipmentAdapter(outlets));

			_items.Clear();
			foreach (var byOutlet in visits.GroupBy(v => v.Outlet))
			{
				var outlet = byOutlet.Key;
				var activities = byOutlet.SelectMany(v => v.Activities).ToList();
				var comments = GetByOutlet(outletComments, outlet);
				var equipments = equipmentHelper.GetByOutlet(outlet);

				_items.Add(new AgendaItem());
			}
		}

		private List<OutletComment> GetByOutlet(List<OutletComment> outletComments, Outlet outlet)
		{
			if (outletComments == null) throw new ArgumentNullException(nameof(outletComments));
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			var byOutlet = new List<OutletComment>();

			foreach (var comment in outletComments)
			{
				if (comment.Outlet == outlet)
				{
					byOutlet.Add(comment);
				}
			}

			return byOutlet;
		}
	}

	public sealed class User : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Picture { get; set; }
	}

	public sealed class UserAdapter : IReadOnlyAdapter<User>
	{
		public Task PopulateAsync(Dictionary<long, User> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class UserHelper : Helper<User>
	{

	}

	public sealed class OutletComment : IDbObject
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public OutletCommentType Type { get; set; }
		public string Contents { get; set; }
		public User CreatedBy { get; set; }
	}

	public sealed class OutletCommentAdapter : IReadOnlyAdapter<OutletComment>, IModifiableAdapter<OutletComment>
	{
		public Dictionary<long, Outlet> Outlets { get; set; }
		public Dictionary<long, OutletCommentType> OutletCommentTypes { get; set; }
		public Dictionary<long, User> Users { get; set; }

		public OutletCommentAdapter(Dictionary<long, Outlet> outlets, Dictionary<long, OutletCommentType> outletCommentTypes, Dictionary<long, User> users)
		{
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));
			if (outletCommentTypes == null) throw new ArgumentNullException(nameof(outletCommentTypes));
			if (users == null) throw new ArgumentNullException(nameof(users));

			Outlets = outlets;
			OutletCommentTypes = outletCommentTypes;
			Users = users;
			throw new NotImplementedException();
		}

		public Task PopulateAsync(Dictionary<long, OutletComment> items)
		{
			throw new NotImplementedException();
		}

		public Task InsertAsync(OutletComment item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(OutletComment item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(OutletComment item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OutletCommentHelper : Helper<OutletComment>
	{
		//public static Task<List<OutletComment>> GetByOutletsAsync(OutletCommentAdapter adapter, Outlet[] outlets)
		//{
		//	if (adapter == null) throw new ArgumentNullException(nameof(adapter));
		//	if (outlets == null) throw new ArgumentNullException(nameof(outlets));

		//	throw new NotImplementedException();
		//}
	}

	public sealed class OutletCommentType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class OutletCommentTypeAdapter : IReadOnlyAdapter<OutletCommentType>
	{
		public Task PopulateAsync(Dictionary<long, OutletCommentType> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OutletCommentTypeHelper : Helper<OutletCommentType>
	{

	}

	public sealed class AgendaItem
	{
		public Outlet Outlet { get; set; }
		public List<Activity> Activities { get; set; }
		public List<OutletComment> OutletComments { get; set; }
		public List<Equipment> Equipments { get; set; }


	}

	public sealed class Outlet : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public List<OutletAssignment> Assignments { get; set; } = new List<OutletAssignment>();

		public bool HasValidAssignment(DateTime date)
		{
			foreach (var a in this.Assignments)
			{
				if (a.IsInValidityPeriod(date))
				{
					return true;
				}
			}

			return false;
		}
	}

	public sealed class OutletAdapter : IReadOnlyAdapter<Outlet>
	{
		private readonly Dictionary<long, List<OutletAssignment>> _assignements;

		public OutletAdapter(Dictionary<long, OutletAssignment> assignements)
		{
			if (assignements == null) throw new ArgumentNullException(nameof(assignements));

			// Create lookup by outlet => list of assignments
			_assignements = assignements.Values
				.GroupBy(v => v.OutletNumber)
				.ToDictionary(v => v.Key, v => v.ToList());
		}

		public Task PopulateAsync(Dictionary<long, Outlet> items)
		{
			// TODO : !!!
			throw new NotImplementedException();
		}
	}

	public sealed class OutletHelper : Helper<Outlet>
	{

	}

	public sealed class OutletAssignment : IDbObject
	{
		public long Id { get; set; }
		public long OutletNumber { get; set; }
		public DateTime FromDate { get; set; } = DateTime.MinValue;
		public DateTime ToDate { get; set; } = DateTime.MaxValue;

		public bool IsInValidityPeriod(DateTime date)
		{
			return this.FromDate <= date && date <= this.ToDate;
		}
	}

	public sealed class OutletAssignmentAdapter : IReadOnlyAdapter<OutletAssignment>
	{
		public Task PopulateAsync(Dictionary<long, OutletAssignment> items)
		{
			// TODO : !!!
			throw new NotImplementedException();
		}
	}

	public sealed class OutletAssignmentHelper : Helper<OutletAssignment>
	{

	}

	public sealed class Visit : IDbObject
	{
		public long Id { get; set; }
		public DateTime Date { get; set; }
		public Outlet Outlet { get; set; }
		public List<Activity> Activities { get; set; } = new List<Activity>();
	}

	public sealed class VisitAdapter : IReadOnlyAdapter<Visit>
	{
		public Dictionary<long, Outlet> Items { get; set; }
		public Dictionary<long, ActivityType> ActivityTypes { get; }
		public Dictionary<long, ActiviStatus> ActiviStatuses { get; set; }

		public VisitAdapter(Dictionary<long, Outlet> items, Dictionary<long, ActivityType> activityTypes, Dictionary<long, ActiviStatus> activiStatuses)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (activityTypes == null) throw new ArgumentNullException(nameof(activityTypes));
			if (activiStatuses == null) throw new ArgumentNullException(nameof(activiStatuses));

			this.Items = items;
			this.ActivityTypes = activityTypes;
			this.ActiviStatuses = activiStatuses;
		}

		public Task PopulateAsync(Dictionary<long, Visit> items)
		{
			throw new NotImplementedException();
		}

		public Task<List<Visit>> GetByDateAsync(DateTime date)
		{
			// TODO : !!!
			throw new NotImplementedException();
		}
	}

	public sealed class Activity : IDbObject
	{
		public long Id { get; set; }
		public ActivityType Type { get; set; }
	}

	public sealed class ActivityType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class ActiviStatus : IDbObject
	{
		public long Id { get; set; }
		public long Name { get; set; }
	}

	public sealed class ActiviStatusAdapter : IReadOnlyAdapter<ActiviStatus>
	{
		public Task PopulateAsync(Dictionary<long, ActiviStatus> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class ActiviStatusHelper : Helper<ActiviStatus>
	{

	}

	public sealed class ActivityTypeAdapter : IReadOnlyAdapter<ActivityType>
	{
		public Task PopulateAsync(Dictionary<long, ActivityType> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class ActivityTypeHelper : Helper<ActivityType>
	{

	}
}