using System;
using System.IO;

namespace Atos.Architecture
{
	public sealed class SourceCodeFile
	{
		public string RootNamespace { get; }
		public string Filename { get; }
		public string Contents { get; }
		private string[] _lines;
		public string[] Lines => _lines ?? (_lines = this.Contents.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
		public string Namespace { get; }

		public SourceCodeFile(string rootNamespace, string filename, string contents)
		{
			if (rootNamespace == null) throw new ArgumentNullException(nameof(rootNamespace));
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			this.RootNamespace = rootNamespace;
			this.Filename = filename;
			this.Contents = contents;

			this.Namespace = rootNamespace;
			var directoryName = Path.GetDirectoryName(filename);
			if (directoryName != string.Empty)
			{
				this.Namespace += @"." + directoryName.Replace(Path.DirectorySeparatorChar, '.');
			}
		}
	}
}