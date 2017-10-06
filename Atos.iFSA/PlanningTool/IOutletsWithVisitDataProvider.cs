using System;
using System.Collections.Generic;
using Atos.Client.Data;

namespace Atos.iFSA.PlanningTool
{
	public interface IOutletsWithVisitDataProvider
	{
		HashSet<long> GetOutletsWithVisit(IDbContext context, DateTime date);
	}
}