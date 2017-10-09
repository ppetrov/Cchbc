using System;
using System.Collections.Generic;

namespace Atos.Architecture
{
	public sealed class SourceCodeRule
	{
		public string Name { get; }
		public List<SourceCodeFile> Violations { get; } = new List<SourceCodeFile>();
		private Func<SourceCodeFile, bool> IsViolated { get; }

		public SourceCodeRule(string name, Func<SourceCodeFile, bool> isViolated)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (isViolated == null) throw new ArgumentNullException(nameof(isViolated));

			this.Name = name;
			this.IsViolated = isViolated;
		}

		public void Apply(SourceCodeFile file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));

			if (this.IsViolated(file))
			{
				this.Violations.Add(file);
			}
		}
	}
}