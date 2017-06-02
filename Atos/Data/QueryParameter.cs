using System;

namespace Atos.Client.Data
{
	public sealed class QueryParameter
	{
		public string Name { get; }
		public object Value { get; set; }

		public QueryParameter(string name, object value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.Value = value;
		}
	}
}