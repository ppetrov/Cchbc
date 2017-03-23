using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cchbc;

namespace iFSA.ArchitectureModule
{
	public sealed class AgendaHeader
	{
		public long Id { get; }
		public string Name { get; }
		public string Address { get; set; }
		public List<AgendaDetail> Details { get; } = new List<AgendaDetail>();

		public AgendaHeader(long id, string name)
		{
			Id = id;
			Name = name;
		}
	}

	public sealed class AgendaDetail
	{
		public long Id { get; }
		public string Name { get; }
		public string Status { get; set; }
		public string Details { get; set; }

		public AgendaDetail(long id, string name, string status, string details)
		{
			this.Id = id;
			this.Name = name;
			this.Status = status;
			this.Details = details;
		}
	}

	public sealed class AgendaHeaderViewModel : ViewModel<AgendaHeader>
	{
		public string Name => this.Model.Name;

		private string _address;
		public string Address
		{
			get { return _address; }
			set
			{
				this.SetProperty(ref _address, value);
				this.Model.Address = value;
			}
		}
		public ObservableCollection<AgendaDetailViewModel> Details { get; } = new ObservableCollection<AgendaDetailViewModel>();

		public AgendaHeaderViewModel(AgendaHeader model) : base(model)
		{
			foreach (var detail in model.Details)
			{
				this.Details.Add(new AgendaDetailViewModel(detail));
			}

			this.Details.CollectionChanged += (sender, args) =>
			{
				this.Model.Details.Clear();
				this.Model.Details.AddRange(this.Details.Select(v => v.Model));
			};
		}
	}

	public sealed class AgendaDetailViewModel : ViewModel<AgendaDetail>
	{
		public string Name => this.Model.Name;

		private string _status;
		public string Status
		{
			get { return _status; }
			set
			{
				this.SetProperty(ref _status, value);
				this.Model.Status = value;
			}
		}

		private string _details;
		public string Details
		{
			get { return _details; }
			set
			{
				this.SetProperty(ref _details, value);
				this.Model.Details = value;
			}
		}

		public AgendaDetailViewModel(AgendaDetail model) : base(model)
		{
			this.Status = model.Status;
			this.Details = model.Details;
		}
	}


	public sealed class AgendaHeaderScreenViewModel : ViewModel
	{
		public IAgendaHeaderScreenDataProvider DataProvider { get; set; }
		private Func<AgendaHeaderViewModel, string, bool> IsTextSearchMatch { get; }
		private List<AgendaHeaderViewModel> AgendaHeaders { get; } = new List<AgendaHeaderViewModel>();

		public ObservableCollection<AgendaHeaderViewModel> CurrentAgendaHeaders { get; } = new ObservableCollection<AgendaHeaderViewModel>();

		private SortOption<AgendaHeaderViewModel> _currentSortOption;
		public SortOption<AgendaHeaderViewModel> CurrentSortOption
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

		public ObservableCollection<SortOption<AgendaHeaderViewModel>> SortOptions { get; } = new ObservableCollection<SortOption<AgendaHeaderViewModel>>();

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

		public AgendaHeaderScreenViewModel(IAgendaHeaderScreenDataProvider dataProvider)
		{
			DataProvider = dataProvider;
			this.SortOptions.Add(new SortOption<AgendaHeaderViewModel>(@"Name", (x, y) =>
			{
				var cmp = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
				if (cmp == 0)
				{
					cmp = x.Model.Id.CompareTo(y.Model.Id);
				}
				return cmp;
			}));

			this.CurrentSortOption = this.SortOptions[0];
			this.IsTextSearchMatch = (vm, search) => vm.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public void Load()
		{
			this.AgendaHeaders.Clear();
			foreach (var agendaHeader in this.DataProvider.GetAgendaHeaders())
			{
				this.AgendaHeaders.Add(new AgendaHeaderViewModel(agendaHeader));
			}
			this.DisplayData();
		}

		public void Add(AgendaHeaderViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.AgendaHeaders.Add(viewModel);
			this.DataProvider.Insert(viewModel.Model);
			this.DisplayData();
		}

		public void Update(AgendaHeaderViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.DataProvider.Update(viewModel.Model);
			this.DisplayData();
		}

		public void Delete(AgendaHeaderViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.AgendaHeaders.Remove(viewModel);
			this.DataProvider.Delete(viewModel.Model);
			this.DisplayData();
		}

		public void Add(AgendaHeaderViewModel header, AgendaDetail detail)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (detail == null) throw new ArgumentNullException(nameof(detail));

			header.Details.Add(new AgendaDetailViewModel(detail));

			this.DataProvider.Insert(detail);

			this.DisplayData();
		}

		public void Update(AgendaDetailViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.DataProvider.Update(viewModel.Model);
			this.DisplayData();
		}

		public void Delete(AgendaDetailViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			foreach (var header in AgendaHeaders)
			{
				foreach (var detail in header.Details)
				{
					if (detail == viewModel)
					{
						header.Details.Remove(detail);
						break;
					}
				}
			}

			this.DataProvider.Delete(viewModel.Model);

			this.DisplayData();
		}

		private void DisplayData()
		{
			this.AgendaHeaders.Sort(this.CurrentSortOption.Comparer);

			this.ApplyCurrentTextSearch();
		}

		private void ApplyCurrentTextSearch()
		{
			this.CurrentAgendaHeaders.Clear();

			foreach (var vm in this.AgendaHeaders)
			{
				if (this.IsTextSearchMatch(vm, this.Search))
				{
					this.CurrentAgendaHeaders.Add(vm);
				}
			}
		}
	}

	public interface IAgendaHeaderScreenDataProvider
	{
		IEnumerable<AgendaHeader> GetAgendaHeaders();

		void Insert(AgendaHeader header);
		void Update(AgendaHeader header);
		void Delete(AgendaHeader header);

		void Insert(AgendaDetail detail);
		void Update(AgendaDetail detail);
		void Delete(AgendaDetail detail);
	}

	public sealed class DebugAgendaHeaderScreenDataProvider : IAgendaHeaderScreenDataProvider
	{
		public IEnumerable<AgendaHeader> GetAgendaHeaders()
		{
			throw new NotImplementedException();
		}

		public void Insert(AgendaHeader header)
		{
			var query = @"insert into AGENDA_HEADERS(ID,NAME,ADDRESS) values(@id,@name,@address)";
			throw new NotImplementedException();
		}

		public void Update(AgendaHeader header)
		{
			var query = @"update AGENDA_HEADERS set name = @name, address = @address where id = @id";
			throw new NotImplementedException();
		}

		public void Delete(AgendaHeader header)
		{
			var query = @"delete from AGENDA_HEADERS where id = @id";
			throw new NotImplementedException();
		}

		public void Insert(AgendaDetail detail)
		{
			var query = @"insert into AGENDA_DETAILS(ID,NAME,STATUS,DETAILS) values ...";
			throw new NotImplementedException();
		}

		public void Update(AgendaDetail detail)
		{
			var query = @"updte AGENDA_DETAILS set name = @name, status = @status, details = @details where id = @id";
			throw new NotImplementedException();
		}

		public void Delete(AgendaDetail detail)
		{
			var query = @"delete from AGENDA_DETAILS where id = @id";
			throw new NotImplementedException();
		}
	}
}