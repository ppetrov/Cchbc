using System;

namespace Cchbc.AppBuilder.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Long = new ClrType(@"long", false, false);
		public static readonly ClrType Decimal = new ClrType(@"decimal", false, false);
		public static readonly ClrType String = new ClrType(@"string", true, false);
		public static readonly ClrType DateTime = new ClrType(@"DateTime", false, false);
		public static readonly ClrType Bytes = new ClrType(@"byte[]", true, false);

		public string Name { get; }
		public bool IsReference { get; }
		public bool IsUserType { get; }

		private ClrType(string name, bool isReference, bool isUserType)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.IsReference = isReference;
			this.IsUserType = isUserType;
		}

		public ClrType(string name, bool isUserType)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.IsReference = true;
			this.IsUserType = isUserType;
		}
	}
}