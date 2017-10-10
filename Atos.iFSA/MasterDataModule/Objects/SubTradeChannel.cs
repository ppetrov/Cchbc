namespace Atos.iFSA.MasterDataModule.Objects
{
	public sealed class SubTradeChannel
	{
		public static readonly SubTradeChannel Empty = new SubTradeChannel(0, string.Empty, string.Empty);

		public long Id { get; }
		public string Name { get; }
		public string Description { get; }

		public SubTradeChannel(long id, string name, string description)
		{
			this.Id = id;
			this.Name = name;
			this.Description = description;
		}
	}
}