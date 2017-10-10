using System;
using System.Collections.ObjectModel;
using Atos.Client;

namespace Atos.iFSA.PlanningTool
{
	public sealed class PlanByIndicatorViewModel : ViewModel
	{
		public ObservableCollection<OutletViewModel> Outlets { get; } = new ObservableCollection<OutletViewModel>();
		public ObservableCollection<PlanIndicatorViewModel> Indicators { get; } = new ObservableCollection<PlanIndicatorViewModel>();

		public ObservableCollection<OutletViewModel> SelectedOutlets { get; } = new ObservableCollection<OutletViewModel>();

		private DateTime _dateTime;
		public DateTime DateTime
		{
			get { return _dateTime; }
			set
			{
				this.SetProperty(ref _dateTime, value);
				this.LoadData(value);
			}
		}

		public void Load(DateTime dateTime)
		{
			this.DateTime = dateTime;
		}

		private void LoadData(DateTime dateTime)
		{
			// TODO : !!!
			// Load outlets

			// TODO : !!!
			// Load indicators

		}
	}
}