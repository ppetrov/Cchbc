namespace Atos.Architecture
{
	public sealed class SourceFile
	{
		public EnumDefinition Enum { get; }
		public InterfaceDefinition Interface { get; }
		public ClassDefinition Class { get; }
	}
}