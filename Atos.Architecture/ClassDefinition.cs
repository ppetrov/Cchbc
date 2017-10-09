using System;

namespace Atos.Architecture
{
	public sealed class ClassDefinition
	{
		public Definition Definition { get; }
		public ClassDefinition Parent { get; }
		public string Name => this.Definition.Name;

		public ClassDefinition(Definition definition, ClassDefinition parent)
		{
			if (definition == null) throw new ArgumentNullException(nameof(definition));

			this.Definition = definition;
			this.Parent = parent;
		}
	}
}