using System;
using System.Collections.Generic;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Objects;
using iFSA.AgendaModule.Objects;

namespace Atos.iFSA.AgendaModule.Data
{
	public sealed class AgendaDataProvider
	{
		public List<AgendaOutlet> GetAgendaOutlets(FeatureContext context, User user, DateTime date)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (user == null) throw new ArgumentNullException(nameof(user));

			return new List<AgendaOutlet>();
		}

		public void Update(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : How to get the Status ???
			//var activityStatus = new ActivityStatus(0, string.Empty);
			//var activity = new Activity(0, outlet, activityType, activityStatus, DateTime.Today, DateTime.Today, string.Empty);

			throw new NotImplementedException();
		}

		public OutletImage GetDefaultOutletImage(MainContext context, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Query Outlet images for the outlet in get the default one
			return null;
		}

		public Activity Insert(FeatureContext context, Activity activity)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query("insert into ...."));

			return null;
		}

		public void Update(FeatureContext context, Activity activity)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			context.Execute(new Query(@"update activities set details = @details, ..... where id = @id"));
		}

		public object GetVisitDay(DateTime dateTime)
		{
			throw new NotImplementedException();
		}
	}
}