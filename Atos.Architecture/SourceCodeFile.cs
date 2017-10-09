using System;

namespace Atos.Architecture
{
	public sealed class SourceCodeFile
	{
		public string Filename { get; }
		public string Contents { get; }
		private string[] _lines;
		public string[] Lines => _lines ?? (_lines = this.Contents.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

		public SourceCodeFile(string filename, string contents)
		{
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			this.Filename = filename;
			this.Contents = contents;
		}
	}
}