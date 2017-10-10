using System;
using System.Collections.Generic;

namespace Atos.Architecture
{
	public sealed class SourceCodeProject
	{
		public string ProjectFilePath { get; }
		public string RootNamespace { get; }
		public List<SourceCodeFile> Files { get; }

		public SourceCodeProject(string projectFilePath, string rootNamespace, List<SourceCodeFile> files)
		{
			if (projectFilePath == null) throw new ArgumentNullException(nameof(projectFilePath));
			if (rootNamespace == null) throw new ArgumentNullException(nameof(rootNamespace));
			if (files == null) throw new ArgumentNullException(nameof(files));

			this.ProjectFilePath = projectFilePath;
			this.RootNamespace = rootNamespace;
			this.Files = files;
		}
	}
}