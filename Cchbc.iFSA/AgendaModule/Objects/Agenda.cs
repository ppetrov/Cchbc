using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cchbc;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Objects
{
	public sealed class Agenda
	{
		private CancellationTokenSource _cts = new CancellationTokenSource();

		public ManualResetEventSlim ImagesLoadedEvent { get; } = new ManualResetEventSlim(false);
		public ConcurrentQueue<OutletImage> OutletImages { get; } = new ConcurrentQueue<OutletImage>();
		public List<AgendaOutlet> Outlets { get; } = new List<AgendaOutlet>();

		public User User { get; private set; }
		public DateTime CurrentDate { get; private set; }

		private Func<MainContext, User, DateTime, List<Visit>> DataProvider { get; }
		private Func<MainContext, HashSet<long>, List<OutletImage>> OutletImagesProvider { get; }

		public Agenda(Func<MainContext, User, DateTime, List<Visit>> dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void LoadDay(MainContext mainContext, User user, DateTime dateTime)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.CurrentDate = dateTime;

			// TODO : !!!
			this.Outlets.Clear();
			foreach (var byOutlet in this.DataProvider(mainContext, user, dateTime).GroupBy(v => v.Outlet))
			{
				var outlet = byOutlet.Key;
				var activities = byOutlet.SelectMany(v => v.Activities).ToList();

				this.Outlets.Add(new AgendaOutlet(outlet, activities));
			}

			// Cancel any pending Images Load
			_cts.Cancel();

			// Start new Images Load
			_cts = new CancellationTokenSource();

			this.ImagesLoadedEvent.Reset();

			var token = _cts.Token;
			Task.Run(() =>
			{
				try
				{
					var cts = token;
					for (var i = 0; i < 10; i++)
					{
						if (cts.IsCancellationRequested)
						{
							break;
						}
						Task.Delay(178).Wait();
						// TODO : Signal data availability
						this.OutletImages.Enqueue(new OutletImage(i + 1, new byte[1024]));
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
				finally
				{
					this.ImagesLoadedEvent.Set();
				}
			}, token);
		}

		public void LoadNextDay(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.LoadDay(mainContext, this.User, this.CurrentDate.AddDays(1));
		}

		public void LoadPreviousDay(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.LoadDay(mainContext, this.User, this.CurrentDate.AddDays(-1));
		}
	}
}