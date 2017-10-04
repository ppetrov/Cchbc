using System;
using System.Collections.Generic;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.Data
{
	public interface IAgendaDataProvider
	{
		List<AgendaOutlet> GetAgendaOutlets(DataQueryContext context, User user, DateTime date);
	}


	public static class AgendaDataProvider
	{
		public static List<AgendaOutlet> GetAgendaOutlets(DataQueryContext context, User user, DateTime date)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (user == null) throw new ArgumentNullException(nameof(user));

			var outlets = context.MainContext.DataCache.GetValues<Outlet>(context.DbContext);

			//context.Execute(new Query<Outlet>("", null));

			return new List<AgendaOutlet>();
		}

		public static void Update(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : How to get the Status ???
			//var activityStatus = new ActivityStatus(0, string.Empty);
			//var activity = new Activity(0, outlet, activityType, activityStatus, DateTime.Today, DateTime.Today, string.Empty);

			throw new NotImplementedException();
		}

		public static Activity Insert(DataQueryContext context, Activity activity)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query("insert into ...."));

			return null;
		}

		public static void Update(DataQueryContext context, Activity activity)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			context.Execute(new Query(@"update activities set details = @details, ..... where id = @id"));
		}

		public static object GetVisitDay(DateTime dateTime)
		{
			throw new NotImplementedException();
		}
	}
}