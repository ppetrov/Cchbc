using System;
using System.Collections.Generic;
using System.Linq;
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

		public List<Visit> GetAll()
		{
			var query = @"SELECT v.Id, v.OutletId, v.Date, a.Id, a.Date, a.ActivityTypeId FROM Visits v INNER JOIN Activities a ON v.Id = a.VisitId";

			var visits = new Dictionary<long, Visit>();
			this.QueryHelper.Fill(query, visits, this.VisitCreator);
			return visits.Values.ToList();
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

		private void VisitCreator(IFieldDataReader r, Dictionary<long, Visit> visits)
		{
			var id = r.GetInt64(0);

			Visit visit;
			if (!visits.TryGetValue(id, out visit))
			{
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
				visit = new Visit(id, outlet, date, new List<Activity>());
				visits.Add(id, visit);
			}
			var activityId = 0L;
			if (!r.IsDbNull(3))
			{
				activityId = r.GetInt64(3);
			}
			var activityDate = DateTime.MinValue;
			if (!r.IsDbNull(4))
			{
				activityDate = r.GetDateTime(4);
			}
			var activityActivityType = default(ActivityType);
			if (!r.IsDbNull(5))
			{
				activityActivityType = this.ActivityTypes[r.GetInt64(5)];
			}

			var activity = new Activity(activityId, activityDate, activityActivityType, visit);
			visit.Activities.Add(activity);
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

	public sealed class ActivityNoteAdapter : IModifiableAdapter<ActivityNote>
	{
		private QueryHelper QueryHelper { get; }
		private Dictionary<long, ActivityNoteType> ActivityNoteTypes { get; }
		private Dictionary<long, Activity> Activities { get; }

		public ActivityNoteAdapter(QueryHelper queryHelper, Dictionary<long, ActivityNoteType> activityNoteTypes, Dictionary<long, Activity> activities)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (activityNoteTypes == null) throw new ArgumentNullException(nameof(activityNoteTypes));
			if (activities == null) throw new ArgumentNullException(nameof(activities));

			this.QueryHelper = queryHelper;
			this.ActivityNoteTypes = activityNoteTypes;
			this.Activities = activities;
		}

		public List<ActivityNote> GetAll()
		{
			var query = @"SELECT Id, Contents, CreatedAt, ActivityNoteTypeId, ActivityId FROM ActivityNotes";

			return this.QueryHelper.Execute(new Query<ActivityNote>(query, this.ActivityNoteCreator));
		}

		public void Insert(ActivityNote item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pContents", item.Contents),
			new QueryParameter(@"@pCreatedAt", item.CreatedAt),
			new QueryParameter(@"@pActivityNoteTypeId", item.ActivityNoteType.Id),
			new QueryParameter(@"@pActivityId", item.Activity.Id),
		};

			var query = @"INSERT INTO ActivityNotes (Contents, CreatedAt, ActivityNoteTypeId, ActivityId) VALUES (@pContents, @pCreatedAt, @pActivityNoteTypeId, @pActivityId)";

			this.QueryHelper.Execute(query, sqlParams);

			item.Id = this.QueryHelper.GetNewId();
		}

		public void Update(ActivityNote item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
			new QueryParameter(@"@pContents", item.Contents),
			new QueryParameter(@"@pCreatedAt", item.CreatedAt),
			new QueryParameter(@"@pActivityNoteTypeId", item.ActivityNoteType.Id),
			new QueryParameter(@"@pActivityId", item.Activity.Id),
		};

			var query = @"UPDATE ActivityNotes SET Contents = @pContents, CreatedAt = @pCreatedAt, ActivityNoteTypeId = @pActivityNoteTypeId, ActivityId = @pActivityId WHERE Id = @pId";

			this.QueryHelper.Execute(query, sqlParams);
		}

		public void Delete(ActivityNote item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
		};

			var query = @"DELETE FROM ActivityNotes WHERE Id = @pId";

			this.QueryHelper.Execute(query, sqlParams);
		}

		private ActivityNote ActivityNoteCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var contents = string.Empty;
			if (!r.IsDbNull(1))
			{
				contents = r.GetString(1);
			}
			var createdAt = DateTime.MinValue;
			if (!r.IsDbNull(2))
			{
				createdAt = r.GetDateTime(2);
			}
			var activityNoteType = default(ActivityNoteType);
			if (!r.IsDbNull(3))
			{
				activityNoteType = this.ActivityNoteTypes[r.GetInt64(3)];
			}
			var activity = default(Activity);
			if (!r.IsDbNull(4))
			{
				activity = this.Activities[r.GetInt64(4)];
			}

			return new ActivityNote(id, contents, createdAt, activityNoteType, activity);
		}
	}
























}