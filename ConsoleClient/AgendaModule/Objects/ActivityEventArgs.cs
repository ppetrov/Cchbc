using System;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule.Objects
{
	public sealed class ActivityEventArgs : EventArgs
	{
		public Activity Activity { get; }

		public ActivityEventArgs(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));
			this.Activity = activity;
		}
	}
}