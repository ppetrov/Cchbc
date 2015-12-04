using System;
using System.IO;
using System.Text;


namespace Cchbc.AppBuilder
{
	public sealed class ClrProject
	{
		private static readonly string Objects = @"Objects";
		private static readonly string Adapters = @"Adapters";
		private static readonly string Helpers = @"Helpers";
		private static readonly string ViewModels = @"ViewModels";
		private static readonly string Modules = @"Modules";

		public void Save(string directoryPath, DbProject project)
		{
			if (directoryPath == null) throw new ArgumentNullException(nameof(directoryPath));
			if (project == null) throw new ArgumentNullException(nameof(project));

			// Create directory names for Objects, Adapters, Helpers, ...
			var objectsPath = Path.Combine(directoryPath, Objects);
			var adaptersPath = Path.Combine(directoryPath, Adapters);
			var helpersPath = Path.Combine(directoryPath, Helpers);
			var modulesPath = Path.Combine(directoryPath, Modules);
			var viewModelsPath = Path.Combine(directoryPath, ViewModels);

			// Create directories
			CreateDirectory(objectsPath);
			CreateDirectory(adaptersPath);
			CreateDirectory(helpersPath);
			CreateDirectory(modulesPath);
			CreateDirectory(viewModelsPath);

			var namespaceName = project.Schema.Name;
			var entities = project.CreateEntities();
			SaveObjects(objectsPath, project, entities, namespaceName);
			SaveAdapters(adaptersPath, project, entities, namespaceName);
			SaveHelpers(helpersPath, project, entities, namespaceName);
			SaveModules(modulesPath, project, entities, namespaceName);
			SaveViewModels(viewModelsPath, project, entities, namespaceName);
		}

		private static void CreateDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
			Directory.CreateDirectory(path);
		}

		private static void SaveObjects(string directoryPath, DbProject project, Entity[] entities, string namespaceName)
		{
			foreach (var entity in entities)
			{
				var className = entity.Class.Name + @".cs";
				var sourceCode = project.CreateEntityClass(entity);

				var buffer = new StringBuilder(sourceCode.Length);
				AddUsingsForObjects(buffer, entity, project);
				AddNamespace(buffer, namespaceName, Objects, sourceCode);

				SaveToFile(directoryPath, className, buffer);
			}
		}

		private static void SaveAdapters(string directoryPath, DbProject project, Entity[] entities, string namespaceName)
		{
			foreach (var entity in entities)
			{
				var className = entity.Class.Name + @"Adapter" + @".cs";
				var sourceCode = project.CreateEntityAdapter(entity);

				var buffer = new StringBuilder(sourceCode.Length);
				AddUsingsForAdapters(buffer, entity, namespaceName);
				AddNamespace(buffer, namespaceName, Adapters, sourceCode);

				SaveToFile(directoryPath, className, buffer);
			}
		}

		private static void SaveHelpers(string directoryPath, DbProject project, Entity[] entities, string namespaceName)
		{
			foreach (var entity in entities)
			{
				// Create helpers only fore ReadOnly tables
				if (project.IsModifiable(entity.Table))
				{
					continue;
				}

				var className = entity.Class.Name + @"Helper" + @".cs";
				var sourceCode = project.CreateEntityHelper(entity);

				var buffer = new StringBuilder(sourceCode.Length);
				AddUsingsForHelpers(buffer, namespaceName);
				AddNamespace(buffer, namespaceName, Helpers, sourceCode);

				SaveToFile(directoryPath, className, buffer);
			}
		}

		private static void SaveModules(string directoryPath, DbProject project, Entity[] entities, string namespaceName)
		{
			foreach (var entity in entities)
			{
				if (project.IsHidden(entity.Table))
				{
					continue;
				}

				var className = entity.Class.Name + @"Module" + @".cs";
				var sourceCode = project.CreateClassModule(entity);

				var buffer = new StringBuilder(sourceCode.Length);
				AddUsingsForModules(namespaceName, buffer, !project.IsModifiable(entity.Table));
				AddNamespace(buffer, namespaceName, Modules, sourceCode);

				SaveToFile(directoryPath, className, buffer);
			}
		}

		private static void SaveViewModels(string directoryPath, DbProject project, Entity[] entities, string namespaceName)
		{
			foreach (var entity in entities)
			{
				if (project.IsHidden(entity.Table))
				{
					continue;
				}				
				SaveClassViewModel(directoryPath, project, entity, namespaceName);

				if (project.HasColumnToInverseTable(entity))
				{
					continue;
				}
				SaveTableViewModel(directoryPath, project, entity, namespaceName);
			}
		}

		private static void SaveClassViewModel(string directoryPath, DbProject project, Entity entity, string namespaceName)
		{
			var className = entity.Class.Name + @"ViewModel" + @".cs";
			var sourceCode = project.CreateClassViewModel(entity);

			var buffer = new StringBuilder(sourceCode.Length);
			AddUsingsForClassViewModels(namespaceName, buffer);
			AddNamespace(buffer, namespaceName, ViewModels, sourceCode);

			SaveToFile(directoryPath, className, buffer);
		}

		private static void SaveTableViewModel(string directoryPath, DbProject project, Entity entity, string namespaceName)
		{
			var className = entity.Table.Name + @"ViewModel" + @".cs";
			var sourceCode = project.CreateTableViewModel(entity);

			var buffer = new StringBuilder(sourceCode.Length);
			AddUsingsForTableViewModels(namespaceName, buffer, !project.IsModifiable(entity.Table));
			AddNamespace(buffer, namespaceName, ViewModels, sourceCode);

			SaveToFile(directoryPath, className, buffer);
		}

		private static void AddUsingsForObjects(StringBuilder buffer, Entity entity, DbProject project)
		{
			var hasReferenceType = false;
			foreach (var clrProperty in entity.Class.Properties)
			{
				if (clrProperty.Type.IsReference)
				{
					hasReferenceType = true;
					break;
				}
			}
			if (hasReferenceType)
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

			// Separate usings from namespace if any usings
			var hasUsings = buffer.Length > 0;
			if (hasUsings)
			{
				buffer.AppendLine();
			}
		}

		private static void AddUsingsForAdapters(StringBuilder buffer, Entity entity, string namespaceName)
		{
			// For ArgumentNullException class
			buffer.AppendLine(@"using System;");

			// For Dictionary<long, T> & List<T> classes
			buffer.AppendLine(@"using System.Collections.Generic;");

			if (entity.InverseTable != null)
			{
				// For .ToList() extension method
				buffer.AppendLine(@"using System.Linq;");
			}

			// For QueryHelper class
			buffer.AppendLine(@"using Cchbc.Data;");

			// For the concrete T class
			buffer.AppendFormat(@"using {0}.Objects;", namespaceName);
			buffer.AppendLine();

			//Separate usings from namespace
			buffer.AppendLine();
		}

		private static void AddUsingsForHelpers(StringBuilder buffer, string namespaceName)
		{
			// For the concrete T class
			buffer.AppendFormat(@"using {0}.Objects;", namespaceName);
			buffer.AppendLine();
			buffer.AppendLine(@"using Cchbc.Helpers;");
			buffer.AppendLine();
		}

		private static void AddUsingsForModules(string namespaceName, StringBuilder buffer, bool readOnly)
		{
			if (readOnly)
			{
				// For the concrete T class
				buffer.AppendFormat(@"using {0}.Objects;", namespaceName);
				buffer.AppendLine();
				buffer.AppendFormat(@"using {0}.ViewModels;", namespaceName);
				buffer.AppendLine();
				buffer.AppendLine(@"using Cchbc;");
				buffer.AppendLine(@"using Cchbc.Search;");
				buffer.AppendLine(@"using Cchbc.Sort;");
			}
			else
			{
				buffer.AppendLine(@"using System;");
				buffer.AppendLine(@"using System.Collections.Generic;");
				buffer.AppendLine(@"using System.Threading.Tasks;");
				buffer.AppendLine(@"using Cchbc;");
				buffer.AppendLine(@"using Cchbc.Features;");
				buffer.AppendLine(@"using Cchbc.Search;");
				buffer.AppendLine(@"using Cchbc.Sort;");
				buffer.AppendLine(@"using Cchbc.Validation;");
				buffer.AppendFormat(@"using {0}.Adapters;", namespaceName);
				buffer.AppendLine();
				buffer.AppendFormat(@"using {0}.Objects;", namespaceName);
				buffer.AppendLine();
				buffer.AppendFormat(@"using {0}.ViewModels;", namespaceName);
				buffer.AppendLine();
			}

			buffer.AppendLine();
		}

		private static void AddUsingsForClassViewModels(string namespaceName, StringBuilder buffer)
		{
			// For the concrete T class
			buffer.AppendFormat(@"using {0}.Objects;", namespaceName);
			buffer.AppendLine();
			buffer.AppendLine(@"using Cchbc.Objects;");
			buffer.AppendLine();
		}

		private static void AddUsingsForTableViewModels(string namespaceName, StringBuilder buffer, bool readOnly)
		{
			if (readOnly)
			{
				buffer.AppendLine(@"using System;");
				buffer.AppendLine(@"using System.Collections.ObjectModel;");
				buffer.AppendLine(@"using Cchbc;");
				buffer.AppendLine(@"using Cchbc.Features;");
				buffer.AppendLine(@"using Cchbc.Objects;");
				buffer.AppendLine(@"using Cchbc.Search;");
				buffer.AppendLine(@"using Cchbc.Sort;");
				buffer.AppendFormat(@"using {0}.Modules;", namespaceName);
				buffer.AppendLine();
				buffer.AppendFormat(@"using {0}.Objects;", namespaceName);
				buffer.AppendLine();
			}
			else
			{
				buffer.AppendLine(@"using System;");
				buffer.AppendLine(@"using System.Collections.ObjectModel;");
				buffer.AppendLine(@"using System.Threading.Tasks;");
				buffer.AppendLine(@"using Cchbc;");
				buffer.AppendLine(@"using Cchbc.Dialog;");
				buffer.AppendLine(@"using Cchbc.Features;");
				buffer.AppendLine(@"using Cchbc.Objects;");
				buffer.AppendLine(@"using Cchbc.Search;");
				buffer.AppendLine(@"using Cchbc.Sort;");
				buffer.AppendFormat(@"using {0}.Adapters;", namespaceName);
				buffer.AppendLine();
				buffer.AppendFormat(@"using {0}.Modules;", namespaceName);
				buffer.AppendLine();
			}

			buffer.AppendLine();
		}

		private static void AddNamespace(StringBuilder buffer, string namespaceName, string name, string sourceCode)
		{
			buffer.Append(@"namespace");
			buffer.Append(' ');
			buffer.Append(namespaceName);
			buffer.Append('.');
			buffer.Append(name);
			buffer.AppendLine();
			buffer.Append('{');
			buffer.AppendLine();

			using (var r = new StringReader(sourceCode))
			{
				string line;
				while ((line = r.ReadLine()) != null)
				{
					if (line != string.Empty)
					{
						buffer.Append('\t');
					}
					buffer.AppendLine(line);
				}
			}

			buffer.Append('}');
			buffer.AppendLine();
		}

		private static void SaveToFile(string directoryPath, string className, StringBuilder buffer)
		{
			File.WriteAllText(Path.Combine(directoryPath, className), buffer.ToString());
		}
	}
}