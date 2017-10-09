using System.Collections.ObjectModel;
using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AddActivityModule
{
	public sealed class ActivityTypeCategoryViewModel : ViewModel
	{
		public string Name { get; }
		public ObservableCollection<ActivityTypeViewModel> Types { get; } = new ObservableCollection<ActivityTypeViewModel>();

		public ActivityTypeCategoryViewModel(ActivityTypeCategory model)
		{
			this.Name = model.Name;
			foreach (var type in model.Types)
			{
				this.Types.Add(new ActivityTypeViewModel(type));
			}
		}
	}
}