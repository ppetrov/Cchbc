using System;

namespace Cchbc.Exceptions
{
	public sealed class ExceptionEntry
	{
		public string Context { get; }
		public string Operation { get; }
		public Exception Exception { get; }

		public ExceptionEntry(string context, string operation, Exception exception)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (operation == null) throw new ArgumentNullException(nameof(operation));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Context = context;
			this.Operation = operation;
			this.Exception = exception;
		}
	}
}