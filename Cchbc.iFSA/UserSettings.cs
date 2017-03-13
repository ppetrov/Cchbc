using Cchbc.iFSA.Objects;
using Cchbc.iFSA.ReplicationModule.Objects;

namespace Cchbc.iFSA
{
	public sealed class UserSettings
	{
		public User User { get; set; }
		public ReplicationConfig ReplicationConfig { get; set; }
	}
}