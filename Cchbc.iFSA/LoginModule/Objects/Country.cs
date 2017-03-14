using System;

namespace iFSA.LoginModule.Objects
{
	public sealed class Country
	{
		public string Name { get; }
		public string Code { get; }

		public Country(string name, string code)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (code == null) throw new ArgumentNullException(nameof(code));

			this.Name = name;
			this.Code = code;
		}
	}
}