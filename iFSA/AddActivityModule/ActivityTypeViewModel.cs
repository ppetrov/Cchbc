using Atos;
using Atos.Client;
using iFSA.Common.Objects;

namespace iFSA.AddActivityModule
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