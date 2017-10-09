using System;

namespace Atos.Architecture
{
	public static class SourceCodeParser
	{
		public static readonly string ClassFlag = @" class ";
		public static readonly string InterfaceFlag = @" interface ";
		public static readonly string EnumFlag = @" enum ";

		public static ClassDefinition ParseClass(string filePath, string contents)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			var definition = Extract(filePath, contents, ClassFlag);
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
				parent = new ClassDefinition(new Definition(string.Empty, parentName, AccessModifier.Public, string.Empty), null);

				definition = new Definition(filePath, selfName, definition.AccessModifier, definition.Body);
			}
			return new ClassDefinition(definition, parent);
		}

		public static InterfaceDefinition ParseInterface(string filePath, string contents)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			var definition = Extract(filePath, contents, InterfaceFlag);
			return definition != null ? new InterfaceDefinition(definition) : null;
		}

		public static EnumDefinition ParseEnum(string filePath, string contents)
		{
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			var definition = Extract(filePath, contents, EnumFlag);
			return definition != null ? new EnumDefinition(definition) : null;
		}

		private static Definition Extract(string filePath, string contents, string flag)
		{
			var matchIndex = contents.IndexOf(flag, StringComparison.OrdinalIgnoreCase);
			if (matchIndex <= 0) return null;

			var index = matchIndex + flag.Length;

			var start = FindStart(contents, matchIndex);
			var end = contents.IndexOf(Environment.NewLine, index, StringComparison.OrdinalIgnoreCase);

			var rawLine = contents.Substring(start, end - start);
			var name = contents.Substring(index, end - index).Trim();
			var accessModifier = ExtractAccessModifier(rawLine);

			var body = ExtractBody(contents, start);

			return new Definition(filePath, name, accessModifier, body);
		}

		private static string ExtractBody(string contents, int index)
		{
			var bodyStart = default(int?);
			var braces = 0;

			for (var i = index; i < contents.Length; i++)
			{
				var symbol = contents[i];
				if (symbol == '{')
				{
					if (!bodyStart.HasValue)
					{
						bodyStart = i;
					}
					braces++;
				}
				if (symbol == '}')
				{
					braces--;
					if (braces == 0)
					{
						var startIndex = bodyStart ?? 0;
						var endIndex = i + 1 - startIndex;
						return contents.Substring(startIndex, endIndex);
					}
				}
			}

			throw new Exception(@"Unable to extract the body");
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

		private static int FindStart(string contents, int index)
		{
			var newLine = Environment.NewLine.ToCharArray();

			for (var i = index - 1; i >= 0; i--)
			{
				if (contents[i] == newLine[1] && contents[i - 1] == newLine[0])
				{
					return i + 1;
				}
			}
			throw new Exception(@"Unable to find the NewLine");
		}
	}
}