namespace iFSA.Common.Objects
{
	public sealed class ActivityStatus
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityStatus(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}
}