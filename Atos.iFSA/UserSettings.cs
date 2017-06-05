using Atos.iFSA.Common.Objects;
using Atos.iFSA.ReplicationModule.Objects;
using iFSA.Common.Objects;

namespace iFSA
{
	public sealed class UserSettings
	{
		public User User { get; set; }
		public ReplicationConfig ReplicationConfig { get; set; }
	}
}