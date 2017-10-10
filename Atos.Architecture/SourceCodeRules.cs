﻿using System;
using System.IO;

namespace Atos.Architecture
{
	public static class SourceCodeRules
	{
		public static SourceCodeRule[] General => new[]
		{
			new SourceCodeRule(@"Multi-Line Comments are denied", file =>
			{
				foreach (var line in file.Lines)
				{
					for (var index = 0; index < line.Length; index++)
					{
						var symbol = line[index];
						if (!char.IsWhiteSpace(symbol) && symbol == '/')
						{
							var nextIndex = index + 1;
							if (nextIndex < line.Length)
							{
								return line[nextIndex] == '*';
							}
						}
					}
				}
				return false;
			}),
			new SourceCodeRule(@"Multiple deinitions class/interface/enum are denied", file =>
			{
				var definitions = 0;

				var flags = new[] {SourceCodeParser.ClassFlag, SourceCodeParser.InterfaceFlag, SourceCodeParser.EnumFlag};
				foreach (var line in file.Lines)
				{
					foreach (var flag in flags)
					{
						definitions += Convert.ToInt32(Convert.ToBoolean(line.IndexOf(flag, StringComparison.OrdinalIgnoreCase) >= 0));
						if (definitions > 1)
						{
							return true;
						}
					}
				}
				return definitions > 1;
			}),
			new SourceCodeRule(@"File Path/Namespace mismatch", file =>
			{
				var flag = @"namespace ";

				foreach (var line in file.Lines)
				{
					foreach (var symbol in line)
					{
						if (!char.IsWhiteSpace(symbol))
						{
							var startIndex = line.IndexOf(flag, StringComparison.OrdinalIgnoreCase);
							if (startIndex >= 0)
							{
								var currentNamespace = line.Substring(startIndex + flag.Length);
								var hasMismatch = !file.Namespace.Equals(currentNamespace, StringComparison.OrdinalIgnoreCase);
								return hasMismatch;
							}
						}
					}
				}
				return false;
			}),

		};
	}
}