using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Localization;
using Cchbc.Logs;
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


	public static class AgendaHeaderDataProvider
	{

	}

	public static class AgendaHeaderValidator
	{
		public static PermissionResult CanAddHeader(MainContext context, AgendaHeader header, IEnumerable<AgendaHeader> headers)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			var result = Validator.ValidateNotEmpty(header.Name, @"HeaderNameIsRequired");
			if (result != PermissionResult.Allow)
			{
				return result;
			}
			var name = (header.Name ?? string.Empty).Trim();
			foreach (var h in headers)
			{
				if (h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return PermissionResult.Deny(@"HeaderWithTheSameNameAlreadyExists");
				}
			}
			var values = GetData(context);

			context.Log(nameof(IsAgendaHeaderExists), LogLevel.Info);
			if (IsAgendaHeaderExists(values, name))
			{
				return PermissionResult.Deny(@"HeaderWithTheSameNameAlreadyExists");
			}
			var date = header.DateTime.Date;
			context.Log(nameof(IsDateExists), LogLevel.Info);
			if (IsDateExists(values, date))
			{
				return PermissionResult.Deny(@"HeaderWithTheSameDateAlreadyExists");
			}
			// TODO : Parameter ???
			if (date < DateTime.Today.AddDays(-30))
			{
				return PermissionResult.Confirm(@"HeaderDateConfirmTooOld");
			}
			return PermissionResult.Allow;
		}

		public static PermissionResult CanUpdateHeaderDate(MainContext context, AgendaHeader header, IEnumerable<AgendaHeader> headers, DateTime newDate)
		{
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			// TODO : !!!
			return PermissionResult.Allow;
		}

		public static PermissionResult CanUpdateHeaderAddress(MainContext context, AgendaHeader header, IEnumerable<AgendaHeader> headers, string address)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (header == null) throw new ArgumentNullException(nameof(header));
			if (headers == null) throw new ArgumentNullException(nameof(headers));

			throw new NotImplementedException();
		}

		private static bool IsAgendaHeaderExists(IEnumerable<string> values, string name)
		{
			foreach (var value in values)
			{
				if (name.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsDateExists(IEnumerable<string> values, DateTime date)
		{
			foreach (var value in values)
			{

			}
			return false;
		}

		private static List<string> GetData(MainContext context)
		{
			// TODO : Query the db
			context.Log(nameof(GetData), LogLevel.Info);
			return new List<string>();
		}
	}


	public static class AgendaHeaderScreen
	{
		public static void Load()
		{
			var context = default(MainContext);
			var viewModel = new AgendaHeaderScreenViewModel(context, default(IAgendaHeaderScreenDataProvider),
				AgendaHeaderValidator.CanAddHeader,
				AgendaHeaderValidator.CanUpdateHeaderDate,
				AgendaHeaderValidator.CanUpdateHeaderAddress
				);
		}
	}

	public interface IAgendaHeaderScreenDataProvider
	{
		IEnumerable<AgendaHeader> GetAgendaHeaders(IDbContext dbContext);

		void Insert(IDbContext dbContext, AgendaHeader header);
		void Update(IDbContext dbContext, AgendaHeader header);
		void Delete(IDbContext dbContext, AgendaHeader header);

		void Insert(IDbContext dbContext, AgendaDetail detail);
		void Update(IDbContext dbContext, AgendaDetail detail);
		void Delete(IDbContext dbContext, AgendaDetail detail);
	}

	public sealed class AgendaHeaderScreenViewModel : ViewModel
	{
		public IAgendaHeaderScreenDataProvider DataProvider { get; set; }
		private Func<AgendaHeaderViewModel, string, bool> IsTextSearchMatch { get; }
		private List<AgendaHeaderViewModel> AgendaHeaders { get; } = new List<AgendaHeaderViewModel>();

		private AgendaHeaderViewModel _selectedAgendaHeader;
		public AgendaHeaderViewModel SelectedAgendaHeader
		{
			get { return _selectedAgendaHeader; }
			set { this.SetProperty(ref _selectedAgendaHeader, value); }
		}
		public ObservableCollection<AgendaHeaderViewModel> CurrentAgendaHeaders { get; } = new ObservableCollection<AgendaHeaderViewModel>();
		private DateTime _agendaHeaderNewDate;
		public DateTime AgendaHeaderNewDate
		{
			get { return _agendaHeaderNewDate; }
			set { this.SetProperty(ref _agendaHeaderNewDate, value); }
		}
		private string _agendaHeaderAddress = string.Empty;
		public string AgendaHeaderAddress
		{
			get { return _agendaHeaderAddress; }
			set { this.SetProperty(ref _agendaHeaderAddress, value); }
		}

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

		public AgendaHeaderScreenViewModel(MainContext mainContext, IAgendaHeaderScreenDataProvider dataProvider,
			Func<MainContext, AgendaHeader, IEnumerable<AgendaHeader>, PermissionResult> canAddHeader,
			Func<MainContext, AgendaHeader, IEnumerable<AgendaHeader>, DateTime, PermissionResult> canUpdateHeaderDate,
			Func<MainContext, AgendaHeader, IEnumerable<AgendaHeader>, string, PermissionResult> canUpdateHeaderAddress)
		{
			DataProvider = dataProvider;
			CanAddHeader = canAddHeader;
			CanUpdateHeaderDate = canUpdateHeaderDate;
			CanUpdateHeaderAddress = canUpdateHeaderAddress;
			MainContext = mainContext;
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
			this.UpdateHeaderDateCommand = new RelayCommand(this.UpdateHeaderDate);
			this.UpdateHeaderAddressCommand = new RelayCommand(this.UpdateHeaderAddress);
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
		public ICommand AddHeaderCommand { get; }
		public ICommand UpdateHeaderDateCommand { get; }
		public ICommand UpdateHeaderAddressCommand { get; }

		private Func<MainContext, AgendaHeader, IEnumerable<AgendaHeader>, PermissionResult> CanAddHeader { get; }
		private Func<MainContext, AgendaHeader, IEnumerable<AgendaHeader>, DateTime, PermissionResult> CanUpdateHeaderDate { get; }
		private Func<MainContext, AgendaHeader, IEnumerable<AgendaHeader>, string, PermissionResult> CanUpdateHeaderAddress { get; }

		private async void AddHeader()
		{
			var context = nameof(AgendaHeaderScreenViewModel);
			var feature = Feature.StartNew(context, nameof(AddHeader));
			try
			{
				// Create model
				var header = new AgendaHeader(0, this.HeaderName) { DateTime = HeaderDateTime };

				var result = this.CanAddHeader(this.MainContext, header, this.AgendaHeaders.Select(v => v.Model));
				if (result.Type != PermissionType.Allow)
				{
					//var message = this.MainContext.LocalizationManager.Get(new LocalizationKey(context, result.LocalizationKeyName));
					//var confirmation = await this.MainContext.ModalDialog.ShowAsync(message, feature, result.Type);
					//if (confirmation != DialogResult.Accept)
					//{
					//	return;
					//}
				}

				// Create view model
				var viewModel = new AgendaHeaderViewModel(header);

				// Insert into db
				using (var ctx = this.MainContext.DbContextCreator())
				{
					this.DataProvider.Insert(ctx, viewModel.Model);
					ctx.Complete();
				}

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

		private async void UpdateHeaderDate()
		{
			var context = nameof(AgendaHeaderScreenViewModel);
			var feature = Feature.StartNew(context, nameof(UpdateHeaderDate));
			try
			{
				var viewModel = this.SelectedAgendaHeader;
				if (viewModel == null)
				{
					this.MainContext.Log(nameof(UpdateHeaderDate) + " without selected header", LogLevel.Warn);
					return;
				}

				// Check newDate and other logic
				var result = this.CanUpdateHeaderDate(this.MainContext, viewModel.Model, this.AgendaHeaders.Select(v => v.Model), this.AgendaHeaderNewDate);
				if (result.Type != PermissionType.Allow)
				{
					//var message = this.MainContext.LocalizationManager.Get(new LocalizationKey(context, result.LocalizationKeyName));
					//var confirmation = await this.MainContext.ModalDialog.ShowAsync(message, feature, result.Type);
					//if (confirmation != DialogResult.Accept)
					//{
					//	return;
					//}
				}

				// Set the new date
				viewModel.DateTime = this.AgendaHeaderNewDate;

				// Update the model
				using (var ctx = this.MainContext.DbContextCreator())
				{
					this.DataProvider.Update(ctx, viewModel.Model);
				}

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

		private async void UpdateHeaderAddress()
		{
			var context = nameof(AgendaHeaderScreenViewModel);
			var feature = Feature.StartNew(context, nameof(UpdateHeaderAddress));
			try
			{
				var viewModel = this.SelectedAgendaHeader;
				if (viewModel == null)
				{
					this.MainContext.Log(nameof(UpdateHeaderAddress) + " without selected header", LogLevel.Warn);
					return;
				}

				// Check newDate and other logic
				var result = this.CanUpdateHeaderAddress(this.MainContext, viewModel.Model, this.AgendaHeaders.Select(v => v.Model), this.AgendaHeaderAddress);
				if (result.Type != PermissionType.Allow)
				{
					//var message = this.MainContext.LocalizationManager.Get(new LocalizationKey(context, result.LocalizationKeyName));
					//var confirmation = await this.MainContext.ModalDialog.ShowAsync(message, feature, result.Type);
					//if (confirmation != DialogResult.Accept)
					//{
					//	return;
					//}
				}

				// Set the new date
				viewModel.Address = this.AgendaHeaderAddress;

				// Update the model
				using (var ctx = this.MainContext.DbContextCreator())
				{
					this.DataProvider.Update(ctx, viewModel.Model);
				}

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
			using (var ctx = this.MainContext.DbContextCreator())
			{
				this.AgendaHeaders.Clear();

				foreach (var agendaHeader in this.DataProvider.GetAgendaHeaders(ctx))
				{
					this.AgendaHeaders.Add(new AgendaHeaderViewModel(agendaHeader));
				}
				ctx.Complete();
			}

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
}