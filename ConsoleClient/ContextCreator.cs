using System;
using Atos.Client.Data;
using ConsoleClient;

namespace Atos.ConsoleClient
{
	public sealed class TransactionContextCreator
	{
		private readonly string _cnString;

		public TransactionContextCreator(string cnString)
		{
			if (cnString == null) throw new ArgumentNullException(nameof(cnString));

			_cnString = cnString;
		}

		public IDbContext Create()
		{
			return new DbContext(_cnString);
		}
	}
}