using System;

namespace Atos.Architecture
{
	public sealed class SourceCodeFile
	{
		public string ProjectPath { get; }
		public string FilePath { get; }
		public string Contents { get; }
		private string[] _lines;
		public string[] Lines => _lines ?? (_lines = this.Contents.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

		public SourceCodeFile(string projectPath, string filePath, string contents)
		{
			if (projectPath == null) throw new ArgumentNullException(nameof(projectPath));
			if (filePath == null) throw new ArgumentNullException(nameof(filePath));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			this.ProjectPath = projectPath;
			this.FilePath = filePath;
			this.Contents = contents;
		}
	}
}