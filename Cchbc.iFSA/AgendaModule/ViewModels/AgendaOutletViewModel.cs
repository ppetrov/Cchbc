using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.ViewModels
{
	public sealed class AgendaOutletViewModel : ViewModel<AgendaOutlet>
	{
		public MainContext Context { get; }

		public Outlet Outlet { get; }
		public long Number { get; }
		public string Name { get; }
		public string Street { get; }
		public string StreetNumber { get; }
		public string City { get; }

		private string _outletImage;
		public string OutletImage
		{
			get { return _outletImage; }
			set { this.SetProperty(ref _outletImage, value); }
		}

		public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();

		public AgendaOutletViewModel(MainContext context, AgendaOutlet model) : base(model)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			var outlet = model.Outlet;
			this.Outlet = outlet;
			this.Number = outlet.Id;
			this.Name = outlet.Name;
			if (outlet.Addresses.Count > 0)
			{
				var address = outlet.Addresses[0];
				this.Street = address.Street;
				this.StreetNumber = address.Number.ToString();
				this.City = address.City;
			}

			foreach (var activity in model.Activities)
			{
				this.Activities.Add(new ActivityViewModel(this, activity));
			}
		}


		public async Task CloseAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			var hasActiveDayBefore = false;
			throw new NotImplementedException();
		}

		public async Task CancelAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			// TODO : !!!
			var cancelReasonSelector = default(Func<Task<CancelReason>>);
			var cancelReason = await cancelReasonSelector();
			if (cancelReason == null)
			{
				return;
			}

			// TODO : !!!
			var cancelOperation = new CalendarCancelOperation(new CalendarDataProvider(this.Context.DbContextCreator));
			cancelOperation.CancelActivities(new[] { activityViewModel.Model }, cancelReason, a =>
			{
				var aid = a.Id;

				var activities = this.Activities;
				for (var i = 0; i < activities.Count; i++)
				{
					var activity = activities[i];
					if (activity.Model.Id == aid)
					{
						activities[i] = new ActivityViewModel(this, a);
						break;
					}
				}
			});
		}

		public void Copy(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		public void Move(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		public void Delete(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}

		public void Execute(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			throw new NotImplementedException();
		}
	}
}