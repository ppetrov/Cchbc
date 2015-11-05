using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.UI
{
	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		private readonly ILogger _logger;
		private readonly List<Login> _logins = new List<Login>();

		public LoginAdapter(ILogger logger)
		{
			_logger = logger;
			_logins.Add(new Login(1, @"Petar", @"123", DateTime.Today.AddDays(-7), true));
			_logins.Add(new Login(1, @"Denis", @"123", DateTime.Today.AddDays(-7), true));
			_logins.Add(new Login(1, @"Teodor", @"123", DateTime.Today.AddDays(-7), true));
		}

		public Task<List<Login>> GetAllAsync()
		{
			return Task.FromResult(new List<Login>(_logins));
		}

		public Task InsertAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var s = Stopwatch.StartNew();
			try
			{
				_logins.Add(item);
				return Task.FromResult(true);
			}
			finally
			{
				_logger.Info($@"{nameof(InsertAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public Task UpdateAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var s = Stopwatch.StartNew();
			try
			{
				return Task.FromResult(true);
			}
			finally
			{
				_logger.Info($@"{nameof(UpdateAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public Task DeleteAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			_logins.Remove(item);

			return Task.FromResult(true);
		}

		public Task<bool> IsReservedAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var isReserved = name.Trim().Equals(@"admin", StringComparison.OrdinalIgnoreCase);
			return Task.FromResult(isReserved);
		}
	}
}