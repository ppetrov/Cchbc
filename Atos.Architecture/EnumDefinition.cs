using System;

namespace Atos.Architecture
{
	public sealed class EnumDefinition
	{
		public Definition Definition { get; }
		public string Name => this.Definition.Name;

		public EnumDefinition(Definition definition)
		{
			if (definition == null) throw new ArgumentNullException(nameof(definition));

			this.Definition = definition;
		}
	}
}