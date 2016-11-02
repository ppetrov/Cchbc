using System;
using Cchbc.Data;

namespace Cchbc.Features.DashboardUI
{
	public sealed class TransactionContextCreator
	{
		private readonly string _cnString;

		public TransactionContextCreator(string cnString)
		{
			if (cnString == null) throw new ArgumentNullException(nameof(cnString));

			_cnString = cnString;
		}

		public ITransactionContext Create()
		{
			return new TransactionContext(_cnString);
		}
	}
}