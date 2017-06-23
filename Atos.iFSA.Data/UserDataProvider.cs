using System;
using System.Collections.Generic;
using System.Linq;
using Atos.Client;
using Atos.Client.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.Data
{
	public static class UserDataProvider
	{
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

			var rows = context.Execute(new Query<UserRow>(@"SELECT U.USER_ID, U.SHORT_NAME, U.ADDRESS, U.E_MAIL, U.FULL_NAME, U.INDUSTRY, U.IS_AUDITTOOL_USER, U.IS_SUPERVISOR, U.PARENT_ID FROM USER_SNAP U",
				r =>
				{
					var id = DbData.GetLong(r, 0);
					var name = DbData.GetString(r, 1);
					var address = DbData.GetString(r, 2);
					var email = DbData.GetString(r, 3);
					var fullName = DbData.GetString(r, 4);
					var industry = DbData.GetString(r, 5);
					var isAuditor = Convert.ToBoolean(DbData.GetInt(r, 6));
					var isTeamLeader = Convert.ToBoolean(DbData.GetInt(r, 7));
					var teamLeaderId = DbData.GetLong(r, 8);
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
