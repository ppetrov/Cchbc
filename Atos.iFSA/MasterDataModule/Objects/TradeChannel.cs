namespace Atos.iFSA.MasterDataModule.Objects
{
	public sealed class TradeChannel
	{
		public static readonly TradeChannel Empty = new TradeChannel(0, string.Empty, string.Empty);

		public long Id { get; }
		public string Name { get; }
		public string Description { get; }

		public TradeChannel(long id, string name, string description)
		{
			this.Id = id;
			this.Name = name;
			this.Description = description;
		}
	}
}