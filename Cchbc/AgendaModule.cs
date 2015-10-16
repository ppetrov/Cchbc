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
		public ILogger Logger { get; }
		public DataCache DataCache { get; } = new DataCache();
		public QueryHelper QueryHelper { get; private set; }

		public Core(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
		}

		public async Task InitializeAsync(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;

			await this.DataCache.LoadAsync(this.Logger, queryHelper.ReadDataQueryHelper);
		}
	}





	public sealed class DataCache
	{
		private readonly UserHelper _userHelper = new UserHelper();
		private readonly OutletHelper _outletHelper = new OutletHelper();
		private readonly OutletCommentTypeHelper _outletOutletCommentTypeHelper = new OutletCommentTypeHelper();
		private readonly ActivityTypeHelper _activityActivityTypeHelper = new ActivityTypeHelper();
		private readonly ActiviStatusHelper _activityStatusHelper = new ActiviStatusHelper();
		private readonly EquipmentTypeHelper _equipmentEquipmentTypeHelper = new EquipmentTypeHelper();
		private readonly OutletCommentHelper _outletCommentHelper = new OutletCommentHelper();

		public UserHelper UserHelper => _userHelper;
		public OutletHelper OutletHelper => _outletHelper;
		public OutletCommentTypeHelper OutletCommentTypeHelper => _outletOutletCommentTypeHelper;
		public ActivityTypeHelper ActivityTypeHelper => _activityActivityTypeHelper;
		public ActiviStatusHelper ActivityStatusHelper => _activityStatusHelper;
		public EquipmentTypeHelper EquipmentTypeHelper => _equipmentEquipmentTypeHelper;
		public OutletCommentHelper OutletCommentHelper => _outletCommentHelper;

		public async Task LoadAsync(ILogger logger, ReadDataQueryHelper queryHelper)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			var s = Stopwatch.StartNew();
			try
			{
				await _userHelper.LoadAsync(new UserAdapter(queryHelper));

				var outletOutletAssignmentHelper = new OutletAssignmentHelper();
				await outletOutletAssignmentHelper.LoadAsync(new OutletAssignmentAdapter(queryHelper));
				await _outletHelper.LoadAsync(new OutletAdapter(queryHelper, outletOutletAssignmentHelper.Items));
				await _outletOutletCommentTypeHelper.LoadAsync(new OutletCommentTypeAdapter(queryHelper));
				await _activityActivityTypeHelper.LoadAsync(new ActivityTypeAdapter(queryHelper));
				await _activityStatusHelper.LoadAsync(new ActiviStatusAdapter(queryHelper));
				await _equipmentEquipmentTypeHelper.LoadAsync(new EquipmentTypeAdapter(queryHelper));
				await _outletCommentHelper.LoadAsync(new OutletCommentAdapter(queryHelper, _outletHelper.Items, _outletOutletCommentTypeHelper.Items, _userHelper.Items));
			}
			finally
			{
				logger.Info($@"{nameof(DataCache)}:{s.ElapsedMilliseconds}ms");
			}
		}
	}

	public sealed class EquipmentType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class EquipmentTypeAdapter : IReadOnlyAdapter<EquipmentType>
	{
		private readonly ReadDataQueryHelper _queryHelper;

		public EquipmentTypeAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, EquipmentType> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			return Task.Delay(1);
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
		private readonly ReadDataQueryHelper _queryHelper;
		private readonly Dictionary<long, Outlet> _outlets;

		public EquipmentAdapter(ReadDataQueryHelper queryHelper, Dictionary<long, Outlet> outlets)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));

			_queryHelper = queryHelper;
			_outlets = outlets;
		}

		public async Task FillAsync(Dictionary<long, Equipment> items)
		{
			await _queryHelper.FillAsync(new Query<Equipment>("", r =>
			{
				var e = new Equipment { Outlet = _outlets[0] };
				return e;
			}), items);
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

		public async Task LoadAsync(DateTime date, DataCache dataCache, ReadDataQueryHelper queryHelper)
		{
			if (dataCache == null) throw new ArgumentNullException(nameof(dataCache));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			var outlets = dataCache.OutletHelper.Items;

			// Remove visits for outlets without assingnments
			var visits = new List<Visit>();
			var visitAdapter = new VisitAdapter(outlets, dataCache.ActivityTypeHelper.Items, dataCache.ActivityStatusHelper.Items);
			foreach (var visit in await visitAdapter.GetByDateAsync(date))
			{
				var outlet = visit.Outlet;
				if (outlet.HasValidAssignment(date))
				{
					visits.Add(visit);
				}
			}

			// Load outlet comments
			var outletComments = dataCache.OutletCommentHelper.Items.Values.ToList();

			// Load equipments
			var equipmentHelper = new EquipmentHelper();
			await equipmentHelper.LoadAsync(new EquipmentAdapter(queryHelper, outlets));

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
		private readonly ReadDataQueryHelper _queryHelper;

		public UserAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public async Task FillAsync(Dictionary<long, User> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			await _queryHelper.FillAsync(new Query<User>(@"SELECT ID, NAME FROM USERS", r =>
			{
				var id = r.GetInt64(0);
				var name = r.GetString(1);
				return new User();
			}), items);
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
		public DateTime CreatedAt { get; set; }
	}

	public sealed class OutletCommentAdapter : IReadOnlyAdapter<OutletComment>, IModifiableAdapter<OutletComment>
	{
		private readonly ReadDataQueryHelper _queryHelper;
		private readonly Dictionary<long, Outlet> _outlets;
		private readonly Dictionary<long, OutletCommentType> _outletCommentTypes;
		private readonly Dictionary<long, User> _users;

		public OutletCommentAdapter(ReadDataQueryHelper queryHelper, Dictionary<long, Outlet> outlets, Dictionary<long, OutletCommentType> outletCommentTypes, Dictionary<long, User> users)
		{
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));
			if (outletCommentTypes == null) throw new ArgumentNullException(nameof(outletCommentTypes));
			if (users == null) throw new ArgumentNullException(nameof(users));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_outlets = outlets;
			_outletCommentTypes = outletCommentTypes;
			_users = users;
			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, OutletComment> items)
		{
			return Task.Delay(1);
		}

		public Task InsertAsync(OutletComment item)
		{
			// TODO : !!! How to query !!!!
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
	}

	public sealed class OutletCommentType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class OutletCommentTypeAdapter : IReadOnlyAdapter<OutletCommentType>
	{
		private readonly ReadDataQueryHelper _queryHelper;

		public OutletCommentTypeAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, OutletCommentType> items)
		{
			return Task.Delay(1);
		}
	}

	public sealed class OutletCommentTypeHelper : Helper<OutletCommentType>
	{

	}

	public sealed class AgendaItem
	{
		public Outlet Outlet { get; set; }
		public List<AgendaActivity> Activities { get; set; }
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
		private readonly ReadDataQueryHelper _queryHelper;
		private readonly Dictionary<long, OutletAssignment[]> _assignements;

		public OutletAdapter(ReadDataQueryHelper queryHelper, Dictionary<long, OutletAssignment> assignements)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (assignements == null) throw new ArgumentNullException(nameof(assignements));

			_queryHelper = queryHelper;

			// Create lookup by outlet => list of assignments
			_assignements = assignements.Values
				.GroupBy(v => v.OutletNumber)
				.ToDictionary(v => v.Key, v => v.ToArray());
		}

		public Task FillAsync(Dictionary<long, Outlet> items)
		{
			return Task.Delay(1);
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
		private readonly ReadDataQueryHelper _queryHelper;

		public OutletAssignmentAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, OutletAssignment> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			return Task.Delay(1);
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
		public List<AgendaActivity> Activities { get; set; } = new List<AgendaActivity>();
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

		public Task FillAsync(Dictionary<long, Visit> items)
		{
			throw new NotImplementedException();
		}

		public Task<List<Visit>> GetByDateAsync(DateTime date)
		{
			// TODO : !!!
			throw new NotImplementedException();
		}
	}

	public sealed class AgendaActivity : IDbObject
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
		private readonly ReadDataQueryHelper _queryHelper;

		public ActiviStatusAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, ActiviStatus> items)
		{
			return Task.Delay(1);
		}
	}

	public sealed class ActiviStatusHelper : Helper<ActiviStatus>
	{

	}

	public sealed class ActivityTypeAdapter : IReadOnlyAdapter<ActivityType>
	{
		private readonly ReadDataQueryHelper _queryHelper;

		public ActivityTypeAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, ActivityType> items)
		{
			return Task.Delay(1);
		}
	}

	public sealed class ActivityTypeHelper : Helper<ActivityType>
	{

	}
}