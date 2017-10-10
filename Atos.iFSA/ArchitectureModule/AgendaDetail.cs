namespace Atos.iFSA.ArchitectureModule
{
	public sealed class AgendaDetail
	{
		public long Id { get; }
		public string Name { get; }
		public string Status { get; set; }
		public string Details { get; set; }

		public AgendaDetail(long id, string name, string status, string details)
		{
			this.Id = id;
			this.Name = name;
			this.Status = status;
			this.Details = details;
		}
	}
}