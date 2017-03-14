namespace iFSA.Common.Objects
{
	public sealed class ActivityStatus
	{
		public long Id { get; }
		public string Name { get; }
		public bool IsActive => this.Id == 0 || this.IsWorking;
		public bool IsWorking => this.Id == 1;

		public ActivityStatus(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}
}