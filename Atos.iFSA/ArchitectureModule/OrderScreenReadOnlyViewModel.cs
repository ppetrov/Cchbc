using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Atos.Client;

namespace Atos.iFSA.ArchitectureModule
{
	public sealed class OrderScreenReadOnlyViewModel : ViewModel
	{
		private Func<OrderHeaderViewModel, string, bool> IsTextSearchMatch { get; }
		private List<OrderHeaderViewModel> OrderHeaders { get; } = new List<OrderHeaderViewModel>();

		public ObservableCollection<OrderHeaderViewModel> CurrentOrderHeaders { get; } = new ObservableCollection<OrderHeaderViewModel>();

		private SortOption<OrderHeaderViewModel> _currentSortOption;
		public SortOption<OrderHeaderViewModel> CurrentSortOption
		{
			get { return _currentSortOption; }
			set
			{
				var descending = (_currentSortOption == value);

				this.SetProperty(ref _currentSortOption, value);

				// Clear the flag for other options
				foreach (var option in this.SortOptions)
				{
					if (option != value)
					{
						option.Ascending = null;
					}
				}

				_currentSortOption.Ascending = !descending;

				this.DisplayData();
			}
		}

		public ObservableCollection<SortOption<OrderHeaderViewModel>> SortOptions { get; } = new ObservableCollection<SortOption<OrderHeaderViewModel>>();

		private string _search = string.Empty;
		public string Search
		{
			get { return _search; }
			set
			{
				this.SetProperty(ref _search, value);
				this.ApplyCurrentTextSearch();
			}
		}

		public OrderScreenReadOnlyViewModel()
		{
			this.SortOptions.Add(new SortOption<OrderHeaderViewModel>(@"Name", (x, y) =>
			{
				var cmp = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
				if (cmp == 0)
				{
					cmp = x.OrderHeader.Id.CompareTo(y.OrderHeader.Id);
				}
				return cmp;
			}));
			this.SortOptions.Add(new SortOption<OrderHeaderViewModel>(@"Created At", (x, y) =>
			{
				var cmp = x.OrderHeader.CreatedAt.CompareTo(y.OrderHeader.CreatedAt);
				if (cmp == 0)
				{
					cmp = x.OrderHeader.Id.CompareTo(y.OrderHeader.Id);
				}
				return cmp;
			}));

			this.CurrentSortOption = this.SortOptions[0];
			this.IsTextSearchMatch = (vm, search) => vm.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
			                                         vm.CreatedAt.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public void Load(IOrderScreenDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.OrderHeaders.Clear();
			foreach (var orderHeader in dataProvider.GetOrderHeaders())
			{
				this.OrderHeaders.Add(new OrderHeaderViewModel(orderHeader));
			}
			this.DisplayData();
		}

		private void DisplayData()
		{
			this.OrderHeaders.Sort(this.CurrentSortOption.Comparer);

			this.ApplyCurrentTextSearch();
		}

		private void ApplyCurrentTextSearch()
		{
			this.CurrentOrderHeaders.Clear();

			foreach (var vm in this.OrderHeaders)
			{
				if (this.IsTextSearchMatch(vm, this.Search))
				{
					this.CurrentOrderHeaders.Add(vm);
				}
			}
		}
	}
}