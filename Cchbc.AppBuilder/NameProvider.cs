using System;
using System.Collections.Generic;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public class NameProvider
	{
		public static readonly string IdName = @"Id";

		public static string LowerFirst(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return char.ToLower(name[0]) + name.Substring(1);
		}

		public Dictionary<string, string> ClassNames { get; } = new Dictionary<string, string>();

		public void AddClassName(DbTable table, string className)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));
			if (className == null) throw new ArgumentNullException(nameof(className));

			this.ClassNames.Add(table.Name, className);
		}

		public string GetClassName(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var name = table.Name;

			string className;
			if (!this.ClassNames.TryGetValue(name, out className) && name.Length > 1)
			{
				className = name.Substring(0, name.Length - 1);
			}

			return className ?? name;
		}
	}
}