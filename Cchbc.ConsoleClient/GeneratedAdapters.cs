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
			var query = @"SELECT v.Id, v.OutletId, v.Date, a.Id, a.Date, a.ActivityTypeId, a.VisitId FROM Visits v INNER JOIN Activities a ON v.Id = a.VisitId";

			var visits = new Dictionary<long, Visit>();
			this.QueryHelper.Fill(query, visits, this.VisitCreator);
			return visits.Values.ToList();
		}




		private void VisitCreator(IFieldDataReader r, Dictionary<long, Visit> visits)
		{
			var id = 0L;
			if (!r.IsDbNull(0))
			{
				id = r.GetInt64(0);
			}

			Visit visit;
			if (!visits.TryGetValue(id, out visit))
			{
				//TODO : Read visit
			}

			//TODO : Read activity
		}

		public void Insert(Visit item)
		{
			throw new NotImplementedException();
		}

		public void Update(Visit item)
		{
			throw new NotImplementedException();
		}

		public void Delete(Visit item)
		{
			throw new NotImplementedException();
		}
	}


































}