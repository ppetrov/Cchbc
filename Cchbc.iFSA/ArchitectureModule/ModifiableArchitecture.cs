﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Validation;

namespace iFSA.ArchitectureModule
{
	public sealed class AgendaHeader
	{
		public long Id { get; set; }
		public string Name { get; }
		public string Address { get; set; }
		public DateTime DateTime { get; set; }
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

		private DateTime _dateTime;
		public DateTime DateTime
		{
			get { return _dateTime; }
			set
			{
				this.SetProperty(ref _dateTime, value);
				this.Model.DateTime = value;
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

	public sealed class AgendaHeaderAddValidatorDataProvider
	{
		public Func<string, bool> AgendaHeaderExists { get; }
		public Func<DateTime, bool> DateExists { get; }

		public AgendaHeaderAddValidatorDataProvider(Func<string, bool> agendaHeaderExists, Func<DateTime, bool> dateExists)
		{
			if (agendaHeaderExists == null) throw new ArgumentNullException(nameof(agendaHeaderExists));
			if (dateExists == null) throw new ArgumentNullException(nameof(dateExists));

			this.AgendaHeaderExists = agendaHeaderExists;
			this.DateExists = dateExists;
		}
	}

	public sealed class ValidatorDataProvider
	{
		private MainContext MainContext { get; }

		private List<string> _values;

		public ValidatorDataProvider(MainContext mainContext)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));

			this.MainContext = mainContext;
		}

		public bool IsAgendaHeaderExists(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.LoadDataIfNotLoaded();
			Debug.WriteLine(@"Find the name in the list");
			return false;
		}

		public bool IsDateExists(DateTime date)
		{
			this.LoadDataIfNotLoaded();
			Debug.WriteLine(@"Find the date in the list");
			return false;
		}

		private void LoadDataIfNotLoaded()
		{
			if (_values == null)
			{
				Debug.WriteLine(@"Load Data");
				_values = new List<string>();
			}
		}
	}

	public static class AgendaHeaderLogic
	{
		public static ValidationResult CanAddHeader(AgendaHeaderAddValidatorDataProvider dataProvider, AgendaHeader header)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (header == null) throw new ArgumentNullException(nameof(header));

			var result = Validator.ValidateNotEmpty(header.Name, @"HeaderNameIsRequired");
			if (result != ValidationResult.Success)
			{
				return result;
			}
			var name = (header.Name ?? string.Empty).Trim();
			if (dataProvider.AgendaHeaderExists(name))
			{
				return new ValidationResult(@"HeaderWithTheSameNameAlreadyExists");
			}
			var date = header.DateTime.Date;
			if (dataProvider.DateExists(date))
			{
				return new ValidationResult(@"HeaderWithTheSameDateAlreadyExists");
			}
			return ValidationResult.Success;
		}
	}

	public static class AgendaHeaderScreen
	{
		public static void Load()
		{
			var viewModel = new AgendaHeaderScreenViewModel(null, default(IAgendaHeaderScreenDataProvider), ctx =>
			{
				var validator = new ValidatorDataProvider(ctx);
				return new AgendaHeaderAddValidatorDataProvider(validator.IsAgendaHeaderExists, validator.IsDateExists);
			});
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

		public AgendaHeaderScreenViewModel(MainContext mainContext, IAgendaHeaderScreenDataProvider dataProvider, Func<MainContext, AgendaHeaderAddValidatorDataProvider> dataProviderCreator)
		{
			DataProvider = dataProvider;
			MainContext = mainContext;
			DataProviderCreator = dataProviderCreator;
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

			this.AddHeaderCommand = new RelayCommand(this.AddHeader);
		}

		private string _headerName = string.Empty;
		public string HeaderName
		{
			get { return _headerName; }
			set { this.SetProperty(ref _headerName, value); }
		}

		private DateTime _headerDateTime;
		public DateTime HeaderDateTime
		{
			get { return _headerDateTime; }
			set { this.SetProperty(ref _headerDateTime, value); }
		}

		public MainContext MainContext { get; }

		public Func<MainContext, AgendaHeaderAddValidatorDataProvider> DataProviderCreator { get; }

		private async void AddHeader()
		{
			var context = nameof(AgendaHeaderScreenViewModel);
			var feature = Feature.StartNew(context, nameof(AddHeader));
			try
			{
				// Create model
				var header = new AgendaHeader(0, this.HeaderName) { DateTime = HeaderDateTime };

				var result = AgendaHeaderLogic.CanAddHeader(this.DataProviderCreator(this.MainContext), header);
				if (result != ValidationResult.Success)
				{
					var message = this.MainContext.LocalizationManager.Get(new LocalizationKey(context, result.LocalizationKeyName));
					await this.MainContext.ModalDialog.ShowAsync(message, feature);
					return;
				}

				// Create view model
				var viewModel = new AgendaHeaderViewModel(header);

				// Insert into db
				this.DataProvider.Insert(viewModel.Model);

				// Insert to view models
				this.AgendaHeaders.Add(viewModel);

				// Refresh the screen
				this.DisplayData();
			}
			catch (Exception ex)
			{
				this.MainContext.FeatureManager.Save(feature, ex);
			}
			finally
			{
				this.MainContext.FeatureManager.Save(feature);
			}
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

		public ICommand AddHeaderCommand { get; }

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
			header.Id = int.MaxValue;
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