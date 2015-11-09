using System;

namespace Cchbc.Features
{
	public sealed class ExceptionEntry
	{
		public string Context { get; }
		public string Name { get; }
		public Exception Exception { get; }

		public ExceptionEntry(string context, string name, Exception exception)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Context = context;
			this.Name = name;
			this.Exception = exception;
		}
	}
}