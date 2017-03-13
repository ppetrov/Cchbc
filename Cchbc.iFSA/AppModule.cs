using System;
using System.Diagnostics;

namespace Cchbc.iFSA
{
	public sealed class AppModule
	{
		public AppContext AppContext { get; }

		public AppModule(AppContext appContext)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));

			this.AppContext = appContext;
		}

		public void Init()
		{
			var cache = this.AppContext.DataCache;
			cache.Register(DataProvider.GetActivityCancelReasons);
			cache.Register(DataProvider.GetActivityCloseReasons);
			cache.Register(DataProvider.GetActivityTypes);
			cache.Register(DataProvider.GetActivityTypeCategories);
			cache.Register(DataProvider.GetActivityStatuses);
		}

		public void Load()
		{
			using (var ctx = this.AppContext.DbContextCreator())
			{
				var cache = this.AppContext.DataCache;

				var s = Stopwatch.StartNew();
				var types = cache.GetValues<ActivityType>(ctx).Values;
				s.Stop();
				//Console.WriteLine(s.ElapsedMilliseconds);
				foreach (var type in types)
				{
					//Console.WriteLine(type.Id + @" " + type.Name);
					//Console.WriteLine(@"Close reasons  " + type.CloseReasons.Count);
					//Console.WriteLine(@"Cancel reasons " + type.CancelReasons.Count);
				}

				var outlet = new Outlet(1, @"Billa");



				ctx.Complete();
			}
		}
	}
}