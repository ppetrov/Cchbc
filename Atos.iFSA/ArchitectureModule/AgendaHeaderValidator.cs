using System;
using System.Collections.Generic;
using Atos.Client;
using Atos.Client.Validation;

namespace Atos.iFSA.ArchitectureModule
{
	public static class AgendaHeaderValidator
	{
		public static PermissionResult CanAddHeader(MainContext context, AgendaHeader header, IEnumerable<AgendaHeader> headers)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			var result = Validator.ValidateNotEmpty(header.Name, @"HeaderNameIsRequired");
			if (result != PermissionResult.Allow)
			{
				return result;
			}
			var name = (header.Name ?? string.Empty).Trim();
			foreach (var h in headers)
			{
				if (h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return PermissionResult.Deny(@"HeaderWithTheSameNameAlreadyExists");
				}
			}
			var values = GetData(context);

			//context.Log(nameof(IsAgendaHeaderExists), LogLevel.Info);
			if (IsAgendaHeaderExists(values, name))
			{
				return PermissionResult.Deny(@"HeaderWithTheSameNameAlreadyExists");
			}
			var date = header.DateTime.Date;
			//context.Log(nameof(IsDateExists), LogLevel.Info);
			if (IsDateExists(values, date))
			{
				return PermissionResult.Deny(@"HeaderWithTheSameDateAlreadyExists");
			}
			// TODO : Parameter ???
			if (date < DateTime.Today.AddDays(-30))
			{
				return PermissionResult.Confirm(@"HeaderDateConfirmTooOld");
			}
			return PermissionResult.Allow;
		}

		public static PermissionResult CanUpdateHeaderDate(MainContext context, AgendaHeader header, IEnumerable<AgendaHeader> headers, DateTime newDate)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			// TODO : !!!
			return PermissionResult.Allow;
		}

		public static PermissionResult CanUpdateHeaderAddress(MainContext context, AgendaHeader header, IEnumerable<AgendaHeader> headers, string address)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			throw new NotImplementedException();
		}

		private static bool IsAgendaHeaderExists(IEnumerable<string> values, string name)
		{
			foreach (var value in values)
			{
				if (name.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsDateExists(IEnumerable<string> values, DateTime date)
		{
			foreach (var value in values)
			{

			}
			return false;
		}

		private static List<string> GetData(MainContext context)
		{
			// TODO : Query the db
			//context.Log(nameof(GetData), LogLevel.Info);
			return new List<string>();
		}
	}
}