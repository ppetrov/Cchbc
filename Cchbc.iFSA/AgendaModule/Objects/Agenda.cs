using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cchbc;
using Cchbc.Logs;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Objects
{
	public sealed class Agenda
	{
		private CancellationTokenSource _cts = new CancellationTokenSource();

		public User User { get; private set; }
		public DateTime CurrentDate { get; private set; }

		public List<AgendaOutlet> Outlets { get; } = new List<AgendaOutlet>();

		public ConcurrentQueue<OutletImage> OutletImages { get; } = new ConcurrentQueue<OutletImage>();
		public ManualResetEventSlim ImagesLoadedEvent { get; } = new ManualResetEventSlim(false);

		private AgendaData Data { get; }

		public Agenda(AgendaData data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			this.Data = data;
		}

		public void LoadDay(MainContext mainContext, User user, DateTime dateTime)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.CurrentDate = dateTime;

			this.Outlets.Clear();
			this.Outlets.AddRange(this.Data.GetAgendaOutlets(mainContext, this.User, dateTime));

			var outlets = new Outlet[this.Outlets.Count];
			for (var index = 0; index < this.Outlets.Count; index++)
			{
				outlets[index] = this.Outlets[index].Outlet;
			}

			// Cancel any pending Images Load
			this._cts.Cancel();

			// Start new Images Load
			this._cts = new CancellationTokenSource();

			Task.Run(() =>
			{
				try
				{
					this.ImagesLoadedEvent.Reset();

					var cts = this._cts;
					foreach (var outlet in outlets)
					{
						if (cts.IsCancellationRequested)
						{
							break;
						}
						var outletImage = this.Data.GetDefaultOutletImage(mainContext, outlet);
						if (outletImage != null)
						{
							this.OutletImages.Enqueue(outletImage);
						}
					}
				}
				catch (Exception ex)
				{
					mainContext.Log(ex.ToString(), LogLevel.Error);
				}
				finally
				{
					this.ImagesLoadedEvent.Set();
				}
			}, this._cts.Token);
		}
	}
}