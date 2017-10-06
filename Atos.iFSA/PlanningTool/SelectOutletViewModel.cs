using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Navigation;
using Atos.Client.PlanningTool;
using Atos.iFSA.Objects;
using Atos.iFSA.PlanningTool.ScreenViewModels;

namespace Atos.iFSA.PlanningTool
{
	public sealed class SelectOutletViewModel : ScreenViewModel
	{
		private List<OutletViewModel> AllOutlets { get; } = new List<OutletViewModel>();

		public string Title { get; } = @"Select Outlet";

		public ObservableCollection<OutletViewModel> Outlets { get; } = new ObservableCollection<OutletViewModel>();
		private OutletViewModel _selectedOutlet;
		public OutletViewModel SelectedOutlet
		{
			get { return _selectedOutlet; }
			set { this.SetProperty(ref _selectedOutlet, value); }
		}

		public ObservableCollection<CategorySearchOption> Options { get; } = new ObservableCollection<CategorySearchOption>();
		private CategorySearchOption _selectedSearchOption;
		public CategorySearchOption SelectedSearchOption
		{
			get { return _selectedSearchOption; }
			set { this.SetProperty(ref _selectedSearchOption, value); }
		}

		private string _search;
		public string Search
		{
			get { return _search; }
			set
			{
				this.SetProperty(ref _search, value);
				this.ApplySearch();
			}
		}

		public ICommand NextCommand { get; }

		public SelectOutletViewModel(MainContext mainContext) : base(mainContext)
		{
			this.Options.Add(new CategorySearchOption(this, @"Today", vi => vi.HasVisitForToday));
			this.Options.Add(new CategorySearchOption(this, @"All", vi => true));
			this.SelectedSearchOption = this.Options[0];

			this.NextCommand = new RelayCommand(this.Next);
		}

		private void Next()
		{
			this.MainContext.ServiceLocator.GetService<INavigationService>().NavigateToAsync<SelectPlanScenarioViewModel>();
		}

		public override Task InitializeAsync(object parameter)
		{
			using (var ctx = this.MainContext.CreateDataQueryContext())
			{
				var outlets = this.MainContext.DataCache.GetValues<Outlet>(ctx.DbContext);

				var outletsWithVisit = this.MainContext.ServiceLocator.GetService<IOutletsWithVisitDataProvider>().GetOutletsWithVisit(ctx.DbContext, DateTime.Today);

				this.AllOutlets.Clear();
				foreach (var outlet in outlets.Values)
				{
					this.AllOutlets.Add(new OutletViewModel(outlet, outletsWithVisit.Contains(outlet.Id)));
				}

				ctx.Complete();
			}

			this.ApplySearch();

			return base.InitializeAsync(parameter);
		}

		public void ApplySearch(CategorySearchOption categorySearchOption)
		{
			if (categorySearchOption == null) throw new ArgumentNullException(nameof(categorySearchOption));

			this.SelectedSearchOption = categorySearchOption;
			this.ApplySearch();
		}

		private void ApplySearch()
		{
			this.Outlets.Clear();
			foreach (var outletViewModel in this.AllOutlets)
			{
				if (outletViewModel.Name.IndexOf(this.Search, StringComparison.OrdinalIgnoreCase) > 0 &&
				    this.SelectedSearchOption.Search(outletViewModel))
					this.Outlets.Add(outletViewModel);
			}
		}
	}
}