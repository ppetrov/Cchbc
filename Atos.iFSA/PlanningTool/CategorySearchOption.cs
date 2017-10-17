using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;

namespace Atos.iFSA.PlanningTool
{
	public sealed class CategorySearchOption : ViewModel
	{
		public SelectOutletViewModel ViewModel { get; }
		public string Name { get; }
		public Func<OutletViewModel, bool> Search { get; }

		public ICommand ApplySearchCommand { get; }

		public CategorySearchOption(SelectOutletViewModel viewModel, string name, Func<OutletViewModel, bool> search)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (search == null) throw new ArgumentNullException(nameof(search));

			this.ViewModel = viewModel;
			this.ApplySearchCommand = new ActionCommand(() => this.ViewModel.ApplySearch(this));
			this.Name = name;
			this.Search = search;
		}
	}
}