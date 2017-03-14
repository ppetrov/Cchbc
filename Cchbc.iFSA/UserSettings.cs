using iFSA.Common.Objects;
using iFSA.ReplicationModule.Objects;

namespace iFSA
{
	public sealed class UserSettings
	{
		public User User { get; set; }
		public ReplicationConfig ReplicationConfig { get; set; }
	}
}