using System.Collections.Generic;
using Atos.Client.Data;

namespace Atos.iFSA.ArchitectureModule
{
	public interface IAgendaHeaderScreenDataProvider
	{
		IEnumerable<AgendaHeader> GetAgendaHeaders(IDbContext dbContext);

		void Insert(IDbContext dbContext, AgendaHeader header);
		void Update(IDbContext dbContext, AgendaHeader header);
		void Delete(IDbContext dbContext, AgendaHeader header);

		void Insert(IDbContext dbContext, AgendaDetail detail);
		void Update(IDbContext dbContext, AgendaDetail detail);
		void Delete(IDbContext dbContext, AgendaDetail detail);
	}
}