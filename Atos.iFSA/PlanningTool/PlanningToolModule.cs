using System;
using System.Collections.ObjectModel;

namespace Atos.Client.PlanningTool
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




	

	public enum PlanIndicatorType
	{
		Activity,
		Volume
	}

	public sealed class PlanIndicator
	{
		public long Id { get; }
		public string Name { get; }
		public PlanIndicatorType Type { get; }

		public PlanIndicator(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class PlanIndicatorViewModel
	{
		public PlanIndicator PlanIndicator { get; }

		public string Name { get; }

		public PlanIndicatorViewModel(PlanIndicator planIndicator)
		{
			if (planIndicator == null) throw new ArgumentNullException(nameof(planIndicator));

			this.PlanIndicator = planIndicator;
			this.Name = planIndicator.Name;
		}
	}


}