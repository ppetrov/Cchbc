using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client.Common;
using Atos.Client.Navigation;
using Atos.Client.PlanningTool.Models;
using Atos.Client.PlanningTool.ViewModels;

namespace Atos.Client.PlanningTool.ScreenViewModels
{
	public sealed class SelectPlanScenarioViewModel : ScreenViewModel
	{
		public string Title { get; } = @"Select Planning Type";
		public ObservableCollection<PlanScenarioViewModel> Scenarios { get; } = new ObservableCollection<PlanScenarioViewModel>();

		private PlanScenarioViewModel _selectedScenario;
		public PlanScenarioViewModel SelectedScenario
		{
			get { return _selectedScenario; }
			set { this.SetProperty(ref _selectedScenario, value); }
		}

		public ICommand NextCommand { get; }

		public SelectPlanScenarioViewModel(MainContext mainContext) : base(mainContext)
		{
			this.NextCommand = new RelayCommand(this.Next);
		}

		private void Next()
		{
			this.MainContext.ServiceLocator.GetService<INavigationService>().NavigateToAsync<SelectOutletViewModel>();
		}

		public override Task InitializeAsync(object parameter)
		{
			this.Scenarios.Clear();
			this.Scenarios.Add(new PlanScenarioViewModel(new PlanScenario(PlanType.ByOutlet, @"Optimized for single outlet, multiple activations")));
			this.Scenarios.Add(new PlanScenarioViewModel(new PlanScenario(PlanType.ByActivation, @"Optimized for single activation, for multiple outlets")));

			return base.InitializeAsync(parameter);
		}
	}
}