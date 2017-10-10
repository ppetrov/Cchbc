using System;
using System.Collections.Generic;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class AgendaHeader
	{
		public long Id { get; set; }
		public string Name { get; }
		public string Address { get; set; }
		public DateTime DateTime { get; set; }
		public List<AgendaDetail> Details { get; } = new List<AgendaDetail>();

		public AgendaHeader(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}