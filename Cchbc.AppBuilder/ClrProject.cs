using System;
using System.IO;
using System.Text;

namespace Cchbc.AppBuilder
{
	public sealed class ClrProject
	{
		private readonly string _objectsName = @"Objects";
		private readonly string _adaptersName = @"Adapters";
		private readonly string _helpersName = @"Helpers";

		public void Save(string dirPath, DbProject project)
		{
			if (dirPath == null) throw new ArgumentNullException(nameof(dirPath));
			if (project == null) throw new ArgumentNullException(nameof(project));

			var entities = project.CreateEntities();

			this.SaveObjects(dirPath, project, entities);
			this.SaveAdapters(dirPath, project, entities);
			this.SaveHelpers(dirPath, project, entities);
			this.SaveManagers();
		}

		private void SaveObjects(string dirPath, DbProject project, Entity[] entities)
		{
			var appName = project.Schema.Name;
			var objectDirectoryName = CreateDirectory(dirPath, _objectsName);

			foreach (var entity in entities)
			{
				var code = project.CreateEntityClass(entity);

				var buffer = new StringBuilder(code.Length);
				if (HasReference(entity))
				{
					// For ArgumentNullException class
					buffer.AppendLine(@"using System;");
				}
				if (project.IsModifiable(entity.Table))
				{
					// For IDbObject interface
					buffer.AppendLine(@"using Cchbc.Objects;");
				}
				if (entity.InverseTable != null)
				{
					// For the List<T> class
					buffer.AppendLine(@"using System.Collections.Generic;");
				}
				var hasUsings = buffer.Length > 0;
				if (hasUsings)
				{
					buffer.AppendLine();
				}

				AppendCode(buffer, appName, _objectsName, code);
				SaveToFile(objectDirectoryName, buffer, entity.Class.Name + @".cs");
			}
		}

		private void SaveAdapters(string dirPath, DbProject project, Entity[] entities)
		{
			var appName = project.Schema.Name;
			var objectDirectoryName = CreateDirectory(dirPath, _adaptersName);

			foreach (var entity in entities)
			{
				var table = entity.Table;
				var code = project.CreateEntityAdapter(entity);

				var buffer = new StringBuilder(code.Length);

				// For ArgumentNullException class
				buffer.AppendLine(@"using System;");

				var useGenerics = true;
				foreach (var e in entities)
				{
					if (e.InverseTable != null && e.InverseTable == table)
					{
						// Inversed table doesn't use generics
						useGenerics = false;
						break;
					}
				}
				if (useGenerics)
				{
					// For Dictionary<long, T> & List<T> classes
					buffer.AppendLine(@"using System.Collections.Generic;");
				}
				if (entity.InverseTable != null)
				{
					// For .ToList() extension method
					buffer.AppendLine(@"using System.Linq;");
				}
				// For QueryHelper class
				buffer.AppendLine(@"using Cchbc.Data;");
				// For the concrete T class
				buffer.AppendFormat(@"using {0}.Objects;", appName);
				buffer.AppendLine();
				buffer.AppendLine();

				AppendCode(buffer, appName, _adaptersName, code);
				SaveToFile(objectDirectoryName, buffer, entity.Class.Name + _adaptersName + @".cs");
			}
		}

		private void SaveHelpers(string dirPath, DbProject project, Entity[] entities)
		{
			//using Cchbc.App.ArticlesModule.Objects;
			//using Cchbc.Helpers;

			//namespace Cchbc.App.ArticlesModule.Helpers
			//	{
			//		public sealed class BrandHelper : Helper<Brand>
			//		{

			//		}
			//	}

			var appName = project.Schema.Name;
			var objectDirectoryName = CreateDirectory(dirPath, _helpersName);

			foreach (var entity in entities)
			{
				var table = entity.Table;
				var code = project.CreateEntityHelper(entity);

				var buffer = new StringBuilder(code.Length);

				// For ArgumentNullException class
				buffer.AppendLine(@"using System;");

				var useGenerics = true;
				foreach (var e in entities)
				{
					if (e.InverseTable != null && e.InverseTable == table)
					{
						// Inversed table doesn't use generics
						useGenerics = false;
						break;
					}
				}
				if (useGenerics)
				{
					// For Dictionary<long, T> & List<T> classes
					buffer.AppendLine(@"using System.Collections.Generic;");
				}
				if (entity.InverseTable != null)
				{
					// For .ToList() extension method
					buffer.AppendLine(@"using System.Linq;");
				}
				// For QueryHelper class
				buffer.AppendLine(@"using Cchbc.Data;");
				// For the concrete T class
				buffer.AppendFormat(@"using {0}.Objects;", appName);
				buffer.AppendLine();
				buffer.AppendLine();

				AppendCode(buffer, appName, _helpersName, code);
				SaveToFile(objectDirectoryName, buffer, entity.Class.Name + _helpersName + @".cs");
			}
		}

		private void SaveManagers()
		{
			//throw new NotImplementedException();
		}

		private static bool HasReference(Entity entity)
		{
			var hasReference = false;

			foreach (var clrProperty in entity.Class.Properties)
			{
				if (clrProperty.Type.IsReference)
				{
					hasReference = true;
					break;
				}
			}

			return hasReference;
		}

		private static string CreateDirectory(string dirPath, string name)
		{
			var objectDirectoryName = Path.Combine(dirPath, name);

			if (Directory.Exists(objectDirectoryName))
			{
				Directory.Delete(objectDirectoryName, true);
			}
			Directory.CreateDirectory(objectDirectoryName);

			return objectDirectoryName;
		}

		private static void AppendCode(StringBuilder buffer, string appName, string name, string code)
		{
			buffer.Append(@"namespace");
			buffer.Append(' ');
			buffer.Append(appName);
			buffer.Append('.');
			buffer.Append(name);
			buffer.AppendLine();
			buffer.AppendLine(@"{");
			using (var sr = new StringReader(code))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if (line != string.Empty)
					{
						line = "\t" + line;
					}
					buffer.AppendLine(line);
				}
			}
			buffer.AppendLine(@"}");
		}

		private static void SaveToFile(string directoryName, StringBuilder buffer, string className)
		{
			File.WriteAllText(Path.Combine(directoryName, className), buffer.ToString());
		}

	}
}