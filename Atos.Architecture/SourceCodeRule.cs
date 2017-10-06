using System;
using System.Collections.Generic;

namespace Atos.Architecture
{
	public sealed class SourceCodeRule
	{
		public string Name { get; }
		public List<string> Violations { get; } = new List<string>();
		private Func<string, bool> IsViolated { get; }

		public SourceCodeRule(string name, Func<string, bool> isViolated)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (isViolated == null) throw new ArgumentNullException(nameof(isViolated));

			this.Name = name;
			this.IsViolated = isViolated;
		}

		public void Apply(string filename, string contents)
		{
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			if (this.IsViolated(contents))
			{
				this.Violations.Add(filename);
			}
		}
	}
}