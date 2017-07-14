namespace ConsoleClient.OrderModule.Models
{
	public sealed class OrderDetail
	{
		public Article Article { get; }
		public int Quanity { get; }

		public OrderDetail(Article article, int quanity)
		{
			this.Article = article;
			this.Quanity = quanity;
		}
	}
}