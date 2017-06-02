using System;

namespace Atos.AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Long = new ClrType(@"long", false);
		public static readonly ClrType Decimal = new ClrType(@"decimal", false);
		public static readonly ClrType String = new ClrType(@"string", true);
		public static readonly ClrType DateTime = new ClrType(@"DateTime", false);
		public static readonly ClrType Bytes = new ClrType(@"byte[]", true);

		public string Name { get; }
		public bool IsReference { get; }
		public bool IsUserType { get; }
		public bool IsCollection { get; }

		private ClrType(string name, bool isReference)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.IsReference = isReference;
			this.IsUserType = false;
			this.IsCollection = false;
		}

		public ClrType(string name, bool isUserType, bool isCollection)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.IsReference = true;
			this.IsUserType = isUserType;
			this.IsCollection = isCollection;
		}
	}
}