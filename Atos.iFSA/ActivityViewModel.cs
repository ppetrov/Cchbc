using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.iFSA.AgendaModule.ViewModels;
using Atos.iFSA.Objects;

namespace Atos.iFSA
{
	public sealed class ActivityViewModel : ViewModel<Activity>
	{
		private AgendaOutletViewModel OutletViewModel { get; }

		public DateTime FromDate { get; }
		public DateTime ToDate { get; }
		public string Type { get; }
		public string Status { get; }
		public string Details { get; }

		public ICommand CloseCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand MoveCommand { get; }
		public ICommand CopyCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand ExecuteCommand { get; }
		public ICommand ChangeFromDateCommand { get; }

		public ActivityViewModel(AgendaOutletViewModel outletViewModel, Activity activity) : base(activity)
		{
			if (outletViewModel == null) throw new ArgumentNullException(nameof(outletViewModel));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			this.OutletViewModel = outletViewModel;
			this.FromDate = activity.FromDate;
			this.ToDate = activity.ToDate;
			this.Type = activity.Type.Name;
			this.Status = activity.Status.Name;
			this.Details = activity.Details;

			this.CloseCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.CloseAsync(this);
			});
			this.CancelCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.CancelAsync(this);
			});
			this.MoveCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Move(this);
			});
			this.CopyCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Copy(this);
			});
			this.DeleteCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Delete(this);
			});
			this.ExecuteCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.Execute(this);
			});
			this.ChangeFromDateCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.ChangeStartTime(this);
			});
		}
	}
}