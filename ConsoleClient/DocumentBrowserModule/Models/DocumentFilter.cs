using System;
using System.Collections.Generic;

namespace ConsoleClient
{
	public sealed class DocumentFilter
	{
		public string Name { get; }
		public DocumentProperty Property { get; }
		public DocumentFilterEntry[] Entries { get; }

		public DocumentFilter(string name, DocumentProperty property, DocumentFilterEntry[] entries)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (entries == null) throw new ArgumentNullException(nameof(entries));

			this.Name = name;
			this.Property = property;
			this.Entries = entries;
		}
	}
}