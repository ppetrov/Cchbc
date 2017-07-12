using System;

namespace ConsoleClient
{
	public sealed class DocumentFilterEntry
	{
		public string Code { get; }
		public string Name { get; }

		public DocumentFilterEntry(string code, string name)
		{
			if (code == null) throw new ArgumentNullException(nameof(code));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Code = code;
			this.Name = name;
		}
	}
}