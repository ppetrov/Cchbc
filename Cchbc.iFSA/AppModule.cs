using System;
using System.Diagnostics;
using Atos;
using iFSA.Common.Objects;

namespace iFSA
{
	public sealed class AppModule
	{
		public MainContext MainContext { get; }

		public AppModule(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public void Init()
		{
			var cache = this.MainContext.DataCache;
			//cache.Register(DataProvider.GetActivityCancelReasons);
			//cache.Register(DataProvider.GetActivityCloseReasons);
			//cache.Register(DataProvider.GetActivityTypes);
			//cache.Register(DataProvider.GetActivityTypeCategories);
			//cache.Register(DataProvider.GetActivityStatuses);
		}

		public void Load()
		{
			using (var ctx = this.MainContext.DbContextCreator())
			{
				var cache = this.MainContext.DataCache;

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