using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cchbc;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Data
{
	public sealed class AgendaDataProvider
	{
		public static List<AgendaOutlet> GetAgendaOutlets(MainContext context, User user, DateTime date)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (user == null) throw new ArgumentNullException(nameof(user));

			Debug.WriteLine(nameof(GetAgendaOutlets));
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

		public static OutletImage GetDefaultOutletImage(MainContext context, Outlet outlet)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Query Outlet images for the outlet in get the default one
			return null;
		}
	}
}