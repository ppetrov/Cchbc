using System;
using System.Collections.ObjectModel;
using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule.ViewModels
{
	public sealed class AgendaOutletViewModel : ViewModel
	{
		private AgendaScreenViewModel ParentViewModel { get; }

		public Outlet Outlet { get; }
		public long Number { get; }
		public string Name { get; }
		public string Street { get; }
		public string StreetNumber { get; }
		public string City { get; }

		public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();

		public AgendaOutletViewModel(AgendaScreenViewModel parentViewModel, AgendaOutlet model)
		{
			if (parentViewModel == null) throw new ArgumentNullException(nameof(parentViewModel));

			this.ParentViewModel = parentViewModel;

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

		public void ChangeStartTime(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			this.ParentViewModel.ChangeStartTime(activityViewModel);
		}

		public void CancelAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			this.ParentViewModel.CancelAsync(activityViewModel);
		}

		public void CloseAsync(ActivityViewModel activityViewModel)
		{
			if (activityViewModel == null) throw new ArgumentNullException(nameof(activityViewModel));

			this.ParentViewModel.CloseAsync(activityViewModel);
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