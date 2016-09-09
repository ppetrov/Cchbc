namespace ConsoleClient.Exceptions
{
	public sealed class FeatureRow
	{
		public long Id { get; }
		public string Name { get; }
		public long ContextId { get; }

		public FeatureRow(long id, string name, long contextId)
		{
			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}
}