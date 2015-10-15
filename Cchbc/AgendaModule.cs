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
		public DataCache DataCache { get; } = new DataCache(new HelperCache());
		public QueryHelper QueryHelper { get; private set; }

		public Core(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
		}

		public async Task Initialize(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;

			await this.DataCache.LoadAsync(this.Logger, queryHelper.ReadDataQueryHelper);
		}
	}

	public sealed class ModifyDataAdapter
	{
		public ModifyDataQueryHelper ModifyDataQueryHelper { get; }

		public ModifyDataAdapter(ModifyDataQueryHelper modifyDataQueryHelper)
		{
			if (modifyDataQueryHelper == null) throw new ArgumentNullException(nameof(modifyDataQueryHelper));

			this.ModifyDataQueryHelper = modifyDataQueryHelper;
		}
	}

	public sealed class ReadDataAdapter
	{
		public ReadDataQueryHelper ReadDataQueryHelper { get; }

		public ReadDataAdapter(ReadDataQueryHelper readDataQueryHelper)
		{
			if (readDataQueryHelper == null) throw new ArgumentNullException(nameof(readDataQueryHelper));

			this.ReadDataQueryHelper = readDataQueryHelper;
		}
	}

	public sealed class DataAdapter
	{
		public QueryHelper QueryHelper { get; }

		public DataAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}
	}

	public sealed class ReadQuery<T>
	{
		public string Statement { get; }
		public Func<IDataReader, T> Creator { get; }
		public QueryParameter[] Parameters { get; }

		public ReadQuery(string statement, Func<IDataReader, T> creator)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (creator == null) throw new ArgumentNullException(nameof(creator));

			this.Statement = statement;
			this.Creator = creator;
			this.Parameters = Enumerable.Empty<QueryParameter>().ToArray();
		}

		public ReadQuery(string statement, Func<IDataReader, T> creator, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (creator == null) throw new ArgumentNullException(nameof(creator));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			this.Statement = statement;
			this.Creator = creator;
			this.Parameters = parameters;
		}
	}

	public sealed class QueryParameter
	{
		public string Name { get; }
		public object Value { get; }

		public QueryParameter(string name, object value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.Value = value;
		}
	}

	public sealed class QueryHelper
	{
		public ReadDataQueryHelper ReadDataQueryHelper { get; }
		public ModifyDataQueryHelper ModifyDataQueryHelper { get; }

		public QueryHelper(ReadDataQueryHelper readDataQueryHelper, ModifyDataQueryHelper modifyDataQueryHelper)
		{
			if (readDataQueryHelper == null) throw new ArgumentNullException(nameof(readDataQueryHelper));
			if (modifyDataQueryHelper == null) throw new ArgumentNullException(nameof(modifyDataQueryHelper));

			this.ReadDataQueryHelper = readDataQueryHelper;
			this.ModifyDataQueryHelper = modifyDataQueryHelper;
		}

		public void Execute(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			ModifyDataQueryHelper.Execute(statement, parameters);
		}

		public Task<List<T>> Execute<T>(ReadQuery<T> query) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return ReadDataQueryHelper.ExecuteAsync(query);
		}

		public void Fill<T>(ReadQuery<T> query, Dictionary<long, T> values) where T : IDbObject
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));

			ReadDataQueryHelper.FillAsync(query, values);
		}
	}

	public abstract class ModifyDataQueryHelper
	{
		public abstract void Execute(string statement, QueryParameter[] parameters);
	}

	public abstract class ReadDataQueryHelper
	{
		public abstract Task<List<T>> ExecuteAsync<T>(ReadQuery<T> query) where T : IDbObject;
		public abstract Task FillAsync<T>(ReadQuery<T> query, Dictionary<long, T> values) where T : IDbObject;
	}

	public interface IDataReader
	{
		bool Read();
		bool IsDbNull(int i);
		int GetInt32(int i);
		long GetInt64(int i);
		decimal GetDecimal(int i);
		string GetString(int i);
		DateTime GetDateTime(int i);
	}

	public sealed class DataCache
	{
		public HelperCache HelperCache { get; }

		public DataCache(HelperCache helperCache)
		{
			if (helperCache == null) throw new ArgumentNullException(nameof(helperCache));

			this.HelperCache = helperCache;
		}

		public async Task LoadAsync(ILogger logger, ReadDataQueryHelper queryHelper)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			var s = Stopwatch.StartNew();
			try
			{
				await this.HelperCache.LoadAsync(logger, queryHelper);
			}
			finally
			{
				logger.Info($@"{nameof(HelperCache)}:{s.ElapsedMilliseconds}ms");
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

			await _userHelper.LoadAsync(new UserAdapter(queryHelper));

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
			await _equipmentEquipmentTypeHelper.LoadAsync(new EquipmentTypeAdapter(queryHelper));

			// Load outlet comments
			await _outletCommentHelper.LoadAsync(new OutletCommentAdapter(_outletHelper.Items, _outletOutletCommentTypeHelper.Items, _userHelper.Items));
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

		public async Task FillAsync(Dictionary<long, EquipmentType> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			await _queryHelper.FillAsync(new ReadQuery<EquipmentType>("", r => null), items);
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
			await _queryHelper.FillAsync(new ReadQuery<Equipment>("", r =>
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

			var helpers = dataCache.HelperCache;
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
			var outletComments = helpers.OutletCommentHelper.Items.Values.ToList();

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

			await _queryHelper.FillAsync(new ReadQuery<User>(@"SELECT ID, NAME FROM USERS", r =>
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
		public Dictionary<long, Outlet> Outlets { get; set; }
		public Dictionary<long, OutletCommentType> OutletCommentTypes { get; set; }
		public Dictionary<long, User> Users { get; set; }

		public OutletCommentAdapter(Dictionary<long, Outlet> outlets, Dictionary<long, OutletCommentType> outletCommentTypes, Dictionary<long, User> users)
		{
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));
			if (outletCommentTypes == null) throw new ArgumentNullException(nameof(outletCommentTypes));
			if (users == null) throw new ArgumentNullException(nameof(users));

			this.Outlets = outlets;
			this.OutletCommentTypes = outletCommentTypes;
			this.Users = users;
		}

		public async Task FillAsync(Dictionary<long, OutletComment> items)
		{
			await Task.Delay(500);
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
		public Task FillAsync(Dictionary<long, OutletCommentType> items)
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

		public Task FillAsync(Dictionary<long, Outlet> items)
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
		public Task FillAsync(Dictionary<long, OutletAssignment> items)
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
		public Task FillAsync(Dictionary<long, ActiviStatus> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class ActiviStatusHelper : Helper<ActiviStatus>
	{

	}

	public sealed class ActivityTypeAdapter : IReadOnlyAdapter<ActivityType>
	{
		public Task FillAsync(Dictionary<long, ActivityType> items)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class ActivityTypeHelper : Helper<ActivityType>
	{

	}
}