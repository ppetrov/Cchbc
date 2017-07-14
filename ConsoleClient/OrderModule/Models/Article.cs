namespace ConsoleClient.OrderModule.Models
{
	public sealed class Article
	{
		public long Id { get; }
		public string Name { get; }

		public Article(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}
}