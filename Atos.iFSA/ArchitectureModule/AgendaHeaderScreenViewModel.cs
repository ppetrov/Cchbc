using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.Client.Features;
using Atos.Client.Validation;

namespace Atos.iFSA.ArchitectureModule
{
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
					cmp = x.AgendaHeader.Id.CompareTo(y.AgendaHeader.Id);
				}
				return cmp;
			}));

			this.CurrentSortOption = this.SortOptions[0];
			this.IsTextSearchMatch = (vm, search) => vm.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

			this.AddHeaderCommand = new ActionCommand(this.AddHeader);
			this.UpdateHeaderDateCommand = new ActionCommand(this.UpdateHeaderDate);
			this.UpdateHeaderAddressCommand = new ActionCommand(this.UpdateHeaderAddress);
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
			var feature = new Feature(context, nameof(AddHeader));
			try
			{
				this.MainContext.Save(feature);

				// Create model
				var header = new AgendaHeader(0, this.HeaderName) { DateTime = HeaderDateTime };

				var result = this.CanAddHeader(this.MainContext, header, this.AgendaHeaders.Select(v => v.AgendaHeader));
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
				using (var ctx = this.MainContext.CreateDataQueryContext())
				{
					//this.DataProvider.Insert(ctx, viewModel.Model);
					ctx.Complete();
				}

				// Insert to view models
				this.AgendaHeaders.Add(viewModel);

				// Refresh the screen
				this.DisplayData();
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private async void UpdateHeaderDate()
		{
			var context = nameof(AgendaHeaderScreenViewModel);
			var feature = new Feature(context, nameof(UpdateHeaderDate));
			try
			{
				this.MainContext.Save(feature);

				var viewModel = this.SelectedAgendaHeader;
				if (viewModel == null)
				{
					//this.MainContext.Log(nameof(UpdateHeaderDate) + " without selected header", LogLevel.Warn);
					return;
				}

				// Check newDate and other logic
				var result = this.CanUpdateHeaderDate(this.MainContext, viewModel.AgendaHeader, this.AgendaHeaders.Select(v => v.AgendaHeader), this.AgendaHeaderNewDate);
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
				//using (var ctx = this.MainContext.DbContextCreator())
				//{
				//	this.DataProvider.Update(ctx, viewModel.Model);
				//}

				// Refresh the screen
				this.DisplayData();
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		private async void UpdateHeaderAddress()
		{
			var context = nameof(AgendaHeaderScreenViewModel);
			var feature = new Feature(context, nameof(UpdateHeaderAddress));
			try
			{
				this.MainContext.Save(feature);

				var viewModel = this.SelectedAgendaHeader;
				if (viewModel == null)
				{
					//this.MainContext.Log(nameof(UpdateHeaderAddress) + " without selected header", LogLevel.Warn);
					return;
				}

				// Check newDate and other logic
				var result = this.CanUpdateHeaderAddress(this.MainContext, viewModel.AgendaHeader, this.AgendaHeaders.Select(v => v.AgendaHeader), this.AgendaHeaderAddress);
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
				//using (var ctx = this.MainContext.DbContextCreator())
				//{
				//	this.DataProvider.Update(ctx, viewModel.Model);
				//}

				// Refresh the screen
				this.DisplayData();
			}
			catch (Exception ex)
			{
				this.MainContext.Save(feature, ex);
			}
		}

		public void Load()
		{
			//using (var ctx = this.MainContext.DbContextCreator())
			//{
			//	this.AgendaHeaders.Clear();

			//	foreach (var agendaHeader in this.DataProvider.GetAgendaHeaders(ctx))
			//	{
			//		this.AgendaHeaders.Add(new AgendaHeaderViewModel(agendaHeader));
			//	}
			//	ctx.Complete();
			//}

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