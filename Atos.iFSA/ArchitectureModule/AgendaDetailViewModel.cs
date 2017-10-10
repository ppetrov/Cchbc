using System;
using Atos.Client;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class AgendaDetailViewModel : ViewModel
	{
		public AgendaDetail AgendaDetail { get; }
		public string Name => this.AgendaDetail.Name;

		private string _status;
		public string Status
		{
			get { return _status; }
			set
			{
				this.SetProperty(ref _status, value);
				this.AgendaDetail.Status = value;
			}
		}

		private string _details;
		public string Details
		{
			get { return _details; }
			set
			{
				this.SetProperty(ref _details, value);
				this.AgendaDetail.Details = value;
			}
		}

		public AgendaDetailViewModel(AgendaDetail agendaDetail)
		{
			if (agendaDetail == null) throw new ArgumentNullException(nameof(agendaDetail));

			this.AgendaDetail = agendaDetail;
			this.Status = agendaDetail.Status;
			this.Details = agendaDetail.Details;
		}
	}
}