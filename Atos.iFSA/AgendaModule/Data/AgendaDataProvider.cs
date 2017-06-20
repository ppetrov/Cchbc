using System;
using System.Collections.Generic;
using System.Diagnostics;
using Atos;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Common.Objects;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Data
{
	public sealed class AgendaDataProvider
	{
		public Activity Insert(Activity activity)
		{
			throw new NotImplementedException();
		}

		public void Update(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : How to get the Status ???
			//var activityStatus = new ActivityStatus(0, string.Empty);
			//var activity = new Activity(0, outlet, activityType, activityStatus, DateTime.Today, DateTime.Today, string.Empty);

			throw new NotImplementedException();
		}

		public List<AgendaOutlet> GetAgendaOutlets(MainContext context, User user, DateTime date)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (user == null) throw new ArgumentNullException(nameof(user));

			// TODO : Sort Outlets
			// TODO : Sort Activities

			//this.Outlets.Clear();
			//foreach (var byOutlet in this.DataProvider.VisitsProvider(mainContext, this.User, this.CurrentDate).GroupBy(v => v.Outlet))
			//{
			//	var outlet = byOutlet.Key;
			//	var activities = byOutlet.SelectMany(v => v.Activities).ToList();

			//	this.Outlets.Add(new AgendaOutlet(outlet, activities));
			//	numbers.Add(outlet.Id);
			//}

			// TODO : Sort outlets
			// TODO : Sort Activities

			using (var ctx = context.DbContextCreator())
			{
				// TODO : !!! Query the database
				//var outlets = context.DataCache.GetValues<Outlet>(ctx);
				//ctx.Complete();
			}

			return new List<AgendaOutlet>();
		}

		public OutletImage GetDefaultOutletImage(MainContext context, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Query Outlet images for the outlet in get the default one
			return null;
		}

		public Activity Insert(MainContext context, Activity activity)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			using (var dbContext = context.DbContextCreator())
			{
				dbContext.Execute(new Query(@"insert into activities values ...."));
				dbContext.Complete();
			}
			return null;
		}

		public void Update(MainContext context, Activity activity)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			using (var dbContext = context.DbContextCreator())
			{
				dbContext.Execute(new Query(@"update activities set details = @details, ..... where id = @id"));
				dbContext.Complete();
			}
		}

		public object GetVisitDay(DateTime dateTime)
		{
			throw new NotImplementedException();
		}
	}
}