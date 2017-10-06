namespace Atos.iFSA.Objects
{
	public sealed class ActivityStatus
	{
		public long Id { get; }
		public string Name { get; }

		public bool IsOpen => this.Id == 0;
		public bool IsWorking => this.Id == 1;
		public bool IsCancel => this.Id == 2;
		public bool IsClose => this.Id == 3;

		public ActivityStatus(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}
}