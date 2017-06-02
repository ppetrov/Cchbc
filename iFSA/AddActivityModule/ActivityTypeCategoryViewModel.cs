using System.Collections.ObjectModel;
using Atos;
using Atos.Client;
using iFSA.Common.Objects;

namespace iFSA.AddActivityModule
{
	public sealed class ActivityTypeCategoryViewModel : ViewModel<ActivityTypeCategory>
	{
		public string Name { get; }
		public ObservableCollection<ActivityTypeViewModel> Types { get; } = new ObservableCollection<ActivityTypeViewModel>();

		public ActivityTypeCategoryViewModel(ActivityTypeCategory model) : base(model)
		{
			this.Name = model.Name;
			foreach (var type in model.Types)
			{
				this.Types.Add(new ActivityTypeViewModel(type));
			}
		}
	}
}