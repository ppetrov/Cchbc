using System;

namespace Cchbc.Features
{
	public sealed class ExceptionEntry : DbEntry
	{
		public Exception Exception { get; }

		public ExceptionEntry(string context, string name, Exception exception) :
			base(context, name)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Exception = exception;
		}
	}
}