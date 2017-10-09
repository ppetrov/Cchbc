using System;

namespace Atos.Architecture
{
	public sealed class Definition
	{
		public string FilePath { get; }
		public string Name { get; }
		public AccessModifier AccessModifier { get; }
		public string Body { get; }

		public Definition(string filePath, string name, AccessModifier accessModifier, string body)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (body == null) throw new ArgumentNullException(nameof(body));

			this.Name = name;
			this.FilePath = filePath;
			this.AccessModifier = accessModifier;
			this.Body = body;
		}
	}
}