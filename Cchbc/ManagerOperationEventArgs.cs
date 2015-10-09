using System;

namespace Cchbc
{
	public sealed class ManagerOperationEventArgs : EventArgs
	{
		public ManagerOperation Operation { get; }

		public ManagerOperationEventArgs(ManagerOperation operation)
		{
			this.Operation = operation;
		}
	}
}