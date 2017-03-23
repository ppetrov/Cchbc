using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cchbc;

namespace iFSA.ArchitectureModule
{
	public sealed class OrderHeader
	{
		public long Id { get; }
		public string Name { get; }
		public DateTime CreatedAt { get; }
		public OrderDetail[] OrderDetails { get; }
	}

	public sealed class OrderDetail
	{
		public long Id { get; }
		public string Name { get; }
		public OrderHeader OrderHeader { get; }
	}

	public sealed class OrderHeaderViewModel : ViewModel<OrderHeader>
	{
		public string Name => this.Model.Name;
		public string CreatedAt => this.Model.CreatedAt.ToString(@"O");
		public OrderDetailViewModel[] Details { get; }

		public OrderHeaderViewModel(OrderHeader model) : base(model)
		{
			this.Details = new OrderDetailViewModel[model.OrderDetails.Length];
			for (var i = 0; i < model.OrderDetails.Length; i++)
			{
				this.Details[i] = new OrderDetailViewModel(model.OrderDetails[i]);
			}
		}
	}

	public sealed class OrderDetailViewModel : ViewModel<OrderDetail>
	{
		public string Name => this.Model.Name;

		public OrderDetailViewModel(OrderDetail model) : base(model)
		{
		}
	}

	public sealed class MasterDetailScreenViewModel : ViewModel
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

		public MasterDetailScreenViewModel()
		{
			this.SortOptions.Add(new SortOption<OrderHeaderViewModel>(@"Name", (x, y) =>
			{
				var cmp = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
				if (cmp == 0)
				{
					cmp = x.Model.Id.CompareTo(y.Model.Id);
				}
				return cmp;
			}));
			this.SortOptions.Add(new SortOption<OrderHeaderViewModel>(@"Created At", (x, y) =>
			{
				var cmp = x.Model.CreatedAt.CompareTo(y.Model.CreatedAt);
				if (cmp == 0)
				{
					cmp = x.Model.Id.CompareTo(y.Model.Id);
				}
				return cmp;
			}));

			this.CurrentSortOption = this.SortOptions[0];
			this.IsTextSearchMatch = (vm, search) => vm.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
													 vm.CreatedAt.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public void Load(IMasterDataProvider masterDataProvider)
		{
			if (masterDataProvider == null) throw new ArgumentNullException(nameof(masterDataProvider));

			this.OrderHeaders.Clear();
			foreach (var orderHeader in masterDataProvider.GetOrderHeaders())
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

	public interface IMasterDataProvider
	{
		IEnumerable<OrderHeader> GetOrderHeaders();
	}
}