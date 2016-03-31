using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.ConsoleClient
{

	public sealed class MasterAdapter
	{
		public void Insert(ITransactionContext ctx)
		{

		}
	}

	public sealed class DetailsAdapter
	{

		public void Insert(ITransactionContext ctx)
		{

		}
	}




	public sealed class Scenario
	{
		private ITransactionContextCreator ContextCreator { get; }

		public Scenario(ITransactionContextCreator contextCreator)
		{
			this.ContextCreator = contextCreator;
		}

		public void Save()
		{
			using (var ctx = this.ContextCreator.Create())
			{
				new MasterAdapter().Insert(ctx);
				new DetailsAdapter().Insert(ctx);

				ctx.Complete();
			}
		}
	}






}