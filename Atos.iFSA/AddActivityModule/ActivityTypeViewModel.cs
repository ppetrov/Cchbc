using System;
using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AddActivityModule
{
	public sealed class ActivityTypeViewModel : ViewModel
	{
		public ActivityType ActivityType { get; }
		public string Name { get; }

		public ActivityTypeViewModel(ActivityType activityType)
		{
			if (activityType == null) throw new ArgumentNullException(nameof(activityType));

			this.ActivityType = activityType;
			this.Name = activityType.Name;
		}
	}
}