namespace Atos.iFSA.LoginModule.Objects
{
	public sealed class UserSettings
	{
		public string Username { get; set; } = string.Empty;
		public string ReplicationHost { get; set; } = string.Empty;
		public int ReplicationPort { get; set; }
	}
}