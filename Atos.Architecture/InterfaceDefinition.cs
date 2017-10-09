using System;

namespace Atos.Architecture
{
	public sealed class InterfaceDefinition
	{
		public Definition Definition { get; }
		public string Name => this.Definition.Name;
		public string Body => this.Definition.Body;

		public InterfaceDefinition(Definition definition)
		{
			if (definition == null) throw new ArgumentNullException(nameof(definition));
			Definition = definition;
		}
	}
}