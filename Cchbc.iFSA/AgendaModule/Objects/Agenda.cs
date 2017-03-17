using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cchbc;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Objects
{
	public sealed class Agenda
	{
		private CancellationTokenSource _cts = new CancellationTokenSource();

		public List<AgendaOutlet> Outlets { get; } = new List<AgendaOutlet>();
		public BlockingCollection<OutletImage> OutletImages { get; } = new BlockingCollection<OutletImage>();

		public User User { get; }
		private AgendaData Data { get; }

		public DateTime CurrentDate { get; private set; }

		public Agenda(User user, AgendaData data)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (data == null) throw new ArgumentNullException(nameof(data));

			this.User = user;
			this.Data = data;
		}

		public void LoadDay(MainContext mainContext, DateTime dateTime)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.CurrentDate = dateTime;

			this.LoadCurrent(mainContext);
		}

		public void LoadNextDay(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.CurrentDate = this.CurrentDate.AddDays(1);

			this.LoadCurrent(mainContext);
		}

		public void LoadPreviousDay(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.CurrentDate = this.CurrentDate.AddDays(-1);

			this.LoadCurrent(mainContext);
		}

		private void LoadCurrent(MainContext mainContext)
		{
			this.Outlets.Clear();
			this.Outlets.AddRange(this.Data.GetAgendaOutlets(mainContext, this.User, this.CurrentDate));

			var outlets = new Outlet[this.Outlets.Count];
			for (var index = 0; index < this.Outlets.Count; index++)
			{
				var agendaOutlet = this.Outlets[index];
				outlets[index] = agendaOutlet.Outlet;
			}

			// Cancel any pending Images Load
			_cts.Cancel();

			// Start new Images Load
			_cts = new CancellationTokenSource();

			Task.Run(() =>
			{
				try
				{
					var cts = _cts;
					foreach (var outlet in outlets)
					{
						if (cts.IsCancellationRequested)
						{
							break;
						}
						var outletImage = this.Data.GetDefaultOutletImage(mainContext, outlet);
						if (outletImage != null)
						{
							this.OutletImages.Add(outletImage);
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
			}, _cts.Token);
		}
	}
}