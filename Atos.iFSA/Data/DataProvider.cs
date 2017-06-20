using System;
using System.Collections.Generic;
using System.Linq;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Common.Objects;

namespace Atos.iFSA.Data
{
	public static class DataProvider
	{
		private static int GetInt(IFieldDataReader r, int index, int defaultValue = 0)
		{
			return r.IsDbNull(index) ? defaultValue : r.GetInt32(index);
		}

		private static long GetLong(IFieldDataReader r, int index, long defaultValue = 0)
		{
			return r.IsDbNull(index) ? defaultValue : r.GetInt64(index);
		}

		private static string GetString(IFieldDataReader r, int index, string defaultValue = "")
		{
			return r.IsDbNull(index) ? defaultValue : r.GetString(index);
		}

		private struct UserRow
		{
			public readonly long Id;
			public readonly string Name;
			public readonly string Address;
			public readonly string Email;
			public readonly string FullName;
			public readonly string Industry;
			public readonly bool IsAuditor;
			public readonly bool IsTeamLeader;
			public readonly long TeamLeaderId;

			public UserRow(long id, string name, string address, string email, string fullName, string industry, bool isAuditor, bool isTeamLeader, long teamLeaderId)
			{
				this.Id = id;
				this.Name = name;
				this.Address = address;
				this.Email = email;
				this.FullName = fullName;
				this.Industry = industry;
				this.IsAuditor = isAuditor;
				this.IsTeamLeader = isTeamLeader;
				this.TeamLeaderId = teamLeaderId;
			}
		}

		public static User[] GetUsers(FeatureContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var users = new Dictionary<long, User>();

			var rows = context.DbContext.Execute(new Query<UserRow>(@"SELECT U.USER_ID, U.SHORT_NAME, U.ADDRESS, U.E_MAIL, U.FULL_NAME, U.INDUSTRY, U.IS_AUDITTOOL_USER, U.IS_SUPERVISOR, U.PARENT_ID FROM USER_SNAP U",
				r =>
				{
					var id = GetLong(r, 0);
					var name = GetString(r, 1);
					var address = GetString(r, 2);
					var email = GetString(r, 3);
					var fullName = GetString(r, 4);
					var industry = GetString(r, 5);
					var isAuditor = Convert.ToBoolean(GetInt(r, 6));
					var isTeamLeader = Convert.ToBoolean(GetInt(r, 7));
					var teamLeaderId = GetLong(r, 8);
					return new UserRow(id, name, address, email, fullName, industry, isAuditor, isTeamLeader, teamLeaderId);
				})).ToList();
			foreach (var row in rows)
			{
				users.Add(row.Id, new User(row.Id, row.Name, row.Address, row.Email, row.FullName, row.Industry, row.IsAuditor, row.IsTeamLeader));
			}
			foreach (var row in rows)
			{
				if (row.TeamLeaderId > 0)
				{
					User teamLeader;
					if (users.TryGetValue(row.TeamLeaderId, out teamLeader))
					{
						User user;
						if (users.TryGetValue(row.Id, out user))
						{
							teamLeader.Users.Add(user);
						}
					}
				}
			}

			var result = new User[users.Count];
			users.Values.CopyTo(result, 0);
			return result;
		}
	}
}