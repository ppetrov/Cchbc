using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.iFSA.AgendaModule.ViewModels;
using iFSA.Common.Objects;

namespace Atos.iFSA
{
	public sealed class ActivityViewModel : ViewModel<Activity>
	{
		public AgendaOutletViewModel OutletViewModel { get; }

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

		public ActivityViewModel(AgendaOutletViewModel outletViewModel, Activity model) : base(model)
		{
			if (outletViewModel == null) throw new ArgumentNullException(nameof(outletViewModel));
			if (model == null) throw new ArgumentNullException(nameof(model));

			this.OutletViewModel = outletViewModel;
			this.FromDate = model.FromDate;
			this.ToDate = model.ToDate;
			this.Type = model.Type.Name;
			this.Status = model.Status.Name;
			this.Details = model.Details;

			this.CloseCommand = new RelayCommand(() =>
			{
				this.OutletViewModel.CloseAsync(this);
			});
			this.CancelCommand = new RelayCommand(async () =>
			{
				await this.OutletViewModel.CancelAsync(this);
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
			this.ChangeFromDateCommand = new RelayCommand(async () =>
			{
				try
				{
					await this.OutletViewModel.ChangeStartTimeAsync(this);
				}
				catch (Exception ex)
				{
					// TODO : Log exception
				}
			});
		}
	}
}