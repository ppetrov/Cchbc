using System;

namespace Cchbc.Db.Clr
{
	public sealed class ClrType
	{
		public static readonly ClrType Long = new ClrType(@"long");
		public static readonly ClrType Decimal = new ClrType(@"decimal");
		public static readonly ClrType String = new ClrType(@"string");
		public static readonly ClrType DateTime = new ClrType(@"DateTime");
		public static readonly ClrType Bytes = new ClrType(@"byte[]");

		public string Name { get; }
		public bool IsReference { get; }
		public bool IsUserType { get; }

		public ClrType(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;			
            this.IsReference = this == String || this == Bytes;
			this.IsUserType = false;
		}

		public ClrType(string name, bool isReference, bool isUserType)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.IsReference = isReference;
			this.IsUserType = isUserType;
		}
	}
}