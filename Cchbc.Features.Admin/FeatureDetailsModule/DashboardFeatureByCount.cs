namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class DashboardFeatureByCount
	{
		public long Id { get; }
		public string Name { get; }
		public int Count { get; }

		public DashboardFeatureByCount(long id, string name, int count)
		{
			this.Id = id;
			this.Name = name;
			this.Count = count;
		}
	}
}