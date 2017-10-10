namespace Atos.iFSA.ArticlesModule.Objects
{
	public sealed class Article
	{
		public long Id { get; }
		public string Name { get; }
		public Brand Brand { get; }
		public Flavor Flavor { get; }
	}
}