using System;

namespace Cchbc
{
	public sealed class ManagerOperationEventArgs : EventArgs
	{
		public ManagerOperation Operation { get; }
		public Exception Exception { get; }

		public ManagerOperationEventArgs(ManagerOperation operation)
		{
			this.Operation = operation;
			this.Exception = null;
		}

		public ManagerOperationEventArgs(ManagerOperation operation, Exception exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Operation = operation;
			this.Exception = exception;
		}

		public ManagerOperationEventArgs WithException(Exception exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			return new ManagerOperationEventArgs(this.Operation, exception);
		}
	}
}