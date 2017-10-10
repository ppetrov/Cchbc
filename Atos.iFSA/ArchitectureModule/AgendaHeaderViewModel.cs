using System;
using System.Collections.ObjectModel;
using System.Linq;
using Atos.Client;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class AgendaHeaderViewModel : ViewModel
	{
		public AgendaHeader AgendaHeader { get; }
		public string Name => this.AgendaHeader.Name;

		private string _address;
		public string Address
		{
			get { return _address; }
			set
			{
				this.SetProperty(ref _address, value);
				this.AgendaHeader.Address = value;
			}
		}

		private DateTime _dateTime;
		public DateTime DateTime
		{
			get { return _dateTime; }
			set
			{
				this.SetProperty(ref _dateTime, value);
				this.AgendaHeader.DateTime = value;
			}
		}

		public ObservableCollection<AgendaDetailViewModel> Details { get; } = new ObservableCollection<AgendaDetailViewModel>();

		public AgendaHeaderViewModel(AgendaHeader agendaHeader)
		{
			if (agendaHeader == null) throw new ArgumentNullException(nameof(agendaHeader));

			this.AgendaHeader = agendaHeader;
			foreach (var detail in agendaHeader.Details)
			{
				this.Details.Add(new AgendaDetailViewModel(detail));
			}

			this.Details.CollectionChanged += (sender, args) =>
			{
				agendaHeader.Details.Clear();
				agendaHeader.Details.AddRange(this.Details.Select(v => v.AgendaDetail));
			};
		}
	}
}