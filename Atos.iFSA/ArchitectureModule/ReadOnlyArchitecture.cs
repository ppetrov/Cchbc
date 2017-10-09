using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Atos.Client;

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
	}

	public sealed class OrderHeaderViewModel : ViewModel
	{
		public OrderHeader OrderHeader { get; }
		public string Name => this.OrderHeader.Name;
		public string CreatedAt => this.OrderHeader.CreatedAt.ToString(@"O");
		public OrderDetailViewModel[] Details { get; }

		public OrderHeaderViewModel(OrderHeader orderHeader)
		{
			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));
			OrderHeader = orderHeader;

			this.Details = new OrderDetailViewModel[orderHeader.OrderDetails.Length];
			for (var i = 0; i < orderHeader.OrderDetails.Length; i++)
			{
				this.Details[i] = new OrderDetailViewModel(orderHeader.OrderDetails[i]);
			}
		}
	}

	public sealed class OrderDetailViewModel : ViewModel
	{
		public OrderDetail OrderDetail { get; }
		public string Name => this.OrderDetail.Name;

		public OrderDetailViewModel(OrderDetail orderDetail)
		{
			if (orderDetail == null) throw new ArgumentNullException(nameof(orderDetail));

			this.OrderDetail = orderDetail;
		}
	}

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

	public sealed class OrderScreenModifiableViewModel : ViewModel
	{
		public IOrderScreenModifiableDataProvider DataProvider { get; set; }
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

		public OrderScreenModifiableViewModel(IOrderScreenModifiableDataProvider dataProvider)
		{
			DataProvider = dataProvider;
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

		public void Load()
		{
			this.OrderHeaders.Clear();
			foreach (var orderHeader in this.DataProvider.GetOrderHeaders())
			{
				this.OrderHeaders.Add(new OrderHeaderViewModel(orderHeader));
			}
			this.DisplayData();
		}

		public void Add(OrderHeaderViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.OrderHeaders.Add(viewModel);

			this.DataProvider.Insert(viewModel.OrderHeader);

			this.DisplayData();
		}

		public void Delete(OrderHeaderViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.OrderHeaders.Remove(viewModel);

			this.DataProvider.Delete(viewModel.OrderHeader);

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

	public interface IOrderScreenDataProvider
	{
		IEnumerable<OrderHeader> GetOrderHeaders();
	}

	public interface IOrderScreenModifiableDataProvider
	{
		IEnumerable<OrderHeader> GetOrderHeaders();
		void Insert(OrderHeader model);
		void Delete(OrderHeader model);
	}


}