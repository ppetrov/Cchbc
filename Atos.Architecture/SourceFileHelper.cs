using System;
using System.Collections.Generic;

namespace Atos.Architecture
{
	public static class SourceFileHelper
	{
		public static ClassDefinition ExtractClass(string filePath, string[] lines)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			var definition = Extract(filePath, lines, @" class ");
			if (definition == null)
			{
				return null;
			}
			var parent = default(ClassDefinition);

			var name = definition.Name;
			var index = name.IndexOf(':');
			if (index >= 0)
			{
				var selfName = name.Substring(0, index).Trim();
				var parentName = name.Substring(index + 1).Trim();
				parent = new ClassDefinition(new Definition(string.Empty, parentName, AccessModifier.Public), null);

				definition = new Definition(filePath, selfName, definition.AccessModifier);
			}
			return new ClassDefinition(definition, parent);
		}

		public static InterfaceDefinition ExtractInterface(string filePath, string[] lines)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			var definition = Extract(filePath, lines, @" interface ");
			return definition != null ? new InterfaceDefinition(definition) : null;
		}

		public static EnumDefinition ExtractEnum(string filePath, string[] lines)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			var definition = Extract(filePath, lines, @" enum ");
			return definition != null ? new EnumDefinition(definition) : null;
		}

		private static Definition Extract(string filePath, string[] lines, string flag)
		{
			var definitionLine = FindDefinitionLine(lines, flag);
			if (definitionLine != null)
			{
				var value = definitionLine.Value;
				var rawLine = value.Key;
				var index = value.Value;

				var name = rawLine.Substring(index + flag.Length).Trim();
				var accessModifier = ExtractAccessModifier(rawLine);

				return new Definition(filePath, name, accessModifier);
			}
			return null;
		}

		private static AccessModifier ExtractAccessModifier(string input)
		{
			for (var i = 0; i < input.Length; i++)
			{
				var symbol = input[i];
				if (!char.IsWhiteSpace(symbol))
				{
					if (input.IndexOf(@"public", i, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return AccessModifier.Public;
					}
					if (input.IndexOf(@"private", i, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return AccessModifier.Private;
					}
					if (input.IndexOf(@"protected", i, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return AccessModifier.Protected;
					}
					if (input.IndexOf(@"internal", i, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return AccessModifier.Internal;
					}
					break;
				}
			}
			return AccessModifier.Public;
		}

		private static KeyValuePair<string, int>? FindDefinitionLine(string[] lines, string flag)
		{
			foreach (var line in lines)
			{
				var start = line.IndexOf(flag, StringComparison.OrdinalIgnoreCase);
				if (start >= 0)
				{
					return new KeyValuePair<string, int>(line, start);
				}
			}
			return null;
		}
	}
}