using System;
using System.Collections.Generic;
using System.Text;

namespace Atos.Architecture
{
	public static class SourceCodeParser
	{
		public static ClassDefinition ParseClass(string filePath, string[] lines)
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
			var hasParent = index >= 0;
			if (hasParent)
			{
				var selfName = name.Substring(0, index).Trim();
				var parentName = name.Substring(index + 1).Trim();
				parent = new ClassDefinition(new Definition(string.Empty, parentName, AccessModifier.Public), null);

				definition = new Definition(filePath, selfName, definition.AccessModifier);
			}
			return new ClassDefinition(definition, parent);
		}

		public static InterfaceDefinition ParseInterface(string filePath, string[] lines)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			var definition = Extract(filePath, lines, @" interface ");
			return definition != null ? new InterfaceDefinition(definition) : null;
		}

		public static EnumDefinition ParseEnum(string filePath, string[] lines)
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
				var lineIndex = value.Key;
				var rawLine = lines[lineIndex];
				var index = value.Value;

				var name = rawLine.Substring(index + flag.Length).Trim();
				var accessModifier = ExtractAccessModifier(rawLine);

				var contents = GetContents(lines, lineIndex);

				return new Definition(filePath, name, accessModifier);
			}
			return null;
		}

		private static string GetContents(string[] lines, int index)
		{
			var buffer = new StringBuilder();

			var start = default(int?);
			var braces = 0;

			for (var i = index; i < lines.Length; i++)
			{
				var line = lines[i];
				if (IsSymbol(line, '{'))
				{
					if (!start.HasValue)
					{
						start = i;
					}
					braces++;
				}
				if (IsSymbol(line, '}'))
				{
					braces--;
					if (braces == 0)
					{
						for (var j = start.Value; j <= i; j++)
						{
							buffer.AppendLine(lines[j]);
						}
						break;
					}
				}
			}

			return buffer.ToString();
		}

		private static bool IsSymbol(string value, char symbol)
		{
			for (var i = 0; i < value.Length; i++)
			{
				var v = value[i];
				if (!char.IsWhiteSpace(v))
				{
					if (v == symbol)
					{
						for (var j = i + 1; j < value.Length; j++)
						{
							if (!char.IsWhiteSpace(value[j]))
							{
								return false;
							}
						}
						return true;
					}
					break;
				}
			}
			return false;
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

		private static KeyValuePair<int, int>? FindDefinitionLine(string[] lines, string flag)
		{
			for (var index = 0; index < lines.Length; index++)
			{
				var start = lines[index].IndexOf(flag, StringComparison.OrdinalIgnoreCase);
				if (start >= 0)
				{
					return new KeyValuePair<int, int>(index, start);
				}
			}
			return null;
		}
	}
}