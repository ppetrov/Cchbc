namespace Cchbc.Db.Clr
{
	public sealed class ClrProperty
	{
		public string Name { get; }
		public ClrType Type { get; }
		public string ParameterName { get; }
		public bool IsReference { get; }

		public ClrProperty(string name, ClrType type, bool isReference)
		{
			this.Name = name;
			this.Type = type;
			this.ParameterName = NameProvider.LowerFirst(name);
			this.IsReference = isReference;
		}
	}
}