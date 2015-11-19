using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{

	public sealed class VisitAdapter : IModifiableAdapter<Visit>
	{
		private QueryHelper QueryHelper { get; }
		private Dictionary<long, Outlet> Outlets { get; }
		private Dictionary<long, ActivityType> ActivityTypes { get; }

		public VisitAdapter(QueryHelper queryHelper, Dictionary<long, Outlet> outlets, Dictionary<long, ActivityType> activityTypes)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));
			if (activityTypes == null) throw new ArgumentNullException(nameof(activityTypes));

			this.QueryHelper = queryHelper;
			this.Outlets = outlets;
			this.ActivityTypes = activityTypes;
		}

		public List<Visit> Get()
		{
			var query = @"SELECT Id, OutletId, Date FROM Visits";

			return this.QueryHelper.Execute(new Query<Visit>(query, r =>
			{
				var id = 0L;
				if (!r.IsDbNull(0))
				{
					id = r.GetInt64(0);
				}

				var outlet = default(Outlet);
				if (!r.IsDbNull(1))
				{
					outlet = this.Outlets[r.GetInt64(1)];
				}

				var date = DateTime.MinValue;
				if (!r.IsDbNull(2))
				{
					date = r.GetDateTime(2);
				}

				var activities = default(List<Activity>);
				if (!r.IsDbNull(3))
				{
					activities = null;
				}

				return new Visit(id, outlet, date, activities);
			}));
		}

		public void Insert(Visit item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pOutletId", item.Outlet.Id),
			new QueryParameter(@"@pDate", item.Date),
		};

			var query = @"INSERT INTO Visits (OutletId, Date) VALUES (@pOutletId, @pDate)";

			this.QueryHelper.Execute(query, sqlParams);

			item.Id = this.QueryHelper.GetNewId();
		}

		public void Update(Visit item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
			new QueryParameter(@"@pOutletId", item.Outlet.Id),
			new QueryParameter(@"@pDate", item.Date),
		};

			var query = @"UPDATE Visits SET OutletId = @pOutletId, Date = @pDate WHERE Id = @pId";

			this.QueryHelper.Execute(query, sqlParams);
		}

		public void Delete(Visit item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
		};

			var query = @"DELETE FROM Visits WHERE Id = @pId";

			this.QueryHelper.Execute(query, sqlParams);
		}
	}



	public sealed class ActivityAdapter : IModifiableAdapter<Activity>
	{
		private QueryHelper QueryHelper { get; }

		public ActivityAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public void Insert(Activity item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pDate", item.Date),
			new QueryParameter(@"@pActivityTypeId", item.ActivityType.Id),
			new QueryParameter(@"@pVisitId", item.Visit.Id),
		};

			var query = @"INSERT INTO Activities (Date, ActivityTypeId, VisitId) VALUES (@pDate, @pActivityTypeId, @pVisitId)";

			this.QueryHelper.Execute(query, sqlParams);

			item.Id = this.QueryHelper.GetNewId();
		}

		public void Update(Activity item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
			new QueryParameter(@"@pDate", item.Date),
			new QueryParameter(@"@pActivityTypeId", item.ActivityType.Id),
			new QueryParameter(@"@pVisitId", item.Visit.Id),
		};

			var query = @"UPDATE Activities SET Date = @pDate, ActivityTypeId = @pActivityTypeId, VisitId = @pVisitId WHERE Id = @pId";

			this.QueryHelper.Execute(query, sqlParams);
		}

		public void Delete(Activity item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
		};

			var query = @"DELETE FROM Activities WHERE Id = @pId";

			this.QueryHelper.Execute(query, sqlParams);
		}
	}






	



















}