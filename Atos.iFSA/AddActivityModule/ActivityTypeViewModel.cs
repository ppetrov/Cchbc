using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AddActivityModule
{
	public sealed class ActivityTypeViewModel : ViewModel<ActivityType>
	{
		public string Name { get; }

		public ActivityTypeViewModel(ActivityType model) : base(model)
		{
			this.Name = model.Name;
		}
	}
}