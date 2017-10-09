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
		};
	}
}