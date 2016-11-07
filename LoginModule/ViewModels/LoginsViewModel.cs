using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using LoginModule.Adapter;
using LoginModule.Objects;

namespace LoginModule.ViewModels
{
	public sealed class LoginsViewModel : ViewModel
	{
		private IModalDialog ModalDialog { get; }
		private FeatureManager FeatureManager { get; }
		private string Context { get; } = nameof(LoginsViewModel);

		public LoginsModule Module { get; }
		public ObservableCollection<LoginViewModel> Logins { get; } = new ObservableCollection<LoginViewModel>();
		public SortOption<LoginViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<LoginViewModel>[] SearchOptions => this.Module.Searcher.Options;

		private bool _isWorking;
		public bool IsWorking
		{
			get { return _isWorking; }
			private set { this.SetProperty(out _isWorking, value); }
		}

		private string _workProgress;
		public string WorkProgress
		{
			get { return _workProgress; }
			private set { this.SetProperty(out _workProgress, value); }
		}

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetProperty(out _textSearch, value);

				var feature = Feature.StartNew(this.Context, nameof(SearchByText));
				this.SearchByText();
				this.FeatureManager.Write(feature);
			}
		}

		private SearchOption<LoginViewModel> _searchOption;
		public SearchOption<LoginViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetProperty(out _searchOption, value);
				var feature = Feature.StartNew(this.Context, nameof(SearchByOption));
				this.SearchByOption();
				this.FeatureManager.Write(feature, value?.Name ?? string.Empty);
			}
		}

		private SortOption<LoginViewModel> _sortOption;
		public SortOption<LoginViewModel> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetProperty(out _sortOption, value);
				var feature = Feature.StartNew(this.Context, nameof(SortBy));
				this.SortBy();
				this.FeatureManager.Write(feature);
			}
		}

		public LoginsViewModel(AppContext appContext, LoginAdapter adapter)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.ModalDialog = appContext.ModalDialog;
			this.FeatureManager = appContext.FeatureManager;
			this.Module = new LoginsModule(appContext.DbContextCreator, adapter, new Sorter<LoginViewModel>(new[]
			{
				new SortOption<LoginViewModel>(string.Empty, (x, y) => 0)
			}), new Searcher<LoginViewModel>((v, s) => false));

			this.Module.OperationStart = feature =>
			{
				//this.FeatureManager.Start(args.Feature);
			};
			this.Module.OperationEnd = feature =>
			{
				this.FeatureManager.Write(feature);
			};
			this.Module.OperationError = (feature, ex) =>
			{
				this.FeatureManager.Write(feature, ex);
			};

			this.Module.ItemInserted = ModuleOnItemInserted;
			this.Module.ItemUpdated = ModuleOnItemUpdated;
			this.Module.ItemDeleted = ModuleOnItemDeleted;
		}

		public async Task InsertAsync(Login login)
		{
			if (login == null) throw new ArgumentNullException(nameof(login));

			var feature = Feature.StartNew(this.Context, nameof(this.InsertAsync));
			try
			{
				var viewModel = new LoginViewModel(login);

				await this.Module.InsertAsync(viewModel, this.ModalDialog, this.AddProgressDisplay(feature));
			}
			catch (Exception ex)
			{
				this.FeatureManager.Write(feature, ex);
			}
		}

		public async Task LoadDataAsync()
		{
			var feature = Feature.StartNew(this.Context, nameof(LoadDataAsync));
			try
			{
				var viewModels = await GetLoginsFromDbAsync();
				this.DisplayLogins(viewModels, feature);

				this.FeatureManager.Write(feature, viewModels.Length.ToString());
			}
			catch (Exception ex)
			{
				this.FeatureManager.Write(feature, ex);
			}
		}

		private async Task<LoginViewModel[]> GetLoginsFromDbAsync()
		{
			var models = await this.Module.GetAllAsync();

			var viewModels = new LoginViewModel[models.Count];

			var index = 0;
			foreach (var model in models)
			{
				viewModels[index++] = new LoginViewModel(model);
			}

			return viewModels;
		}

		public Task UpdateAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.UpdateAsync(viewModel, dialog, this.AddProgressDisplay(Feature.StartNew(this.Context, nameof(UpdateAsync))));
		}

		public Task DeleteAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.DeleteAsync(viewModel, dialog, this.AddProgressDisplay(Feature.StartNew(this.Context, nameof(DeleteAsync))));
		}

		private void ModuleOnItemInserted(ViewModelEventArgs<LoginViewModel> args)
		{
			this.Module.Insert(this.Logins, args.ViewModel, this.TextSearch, this.SearchOption);
		}

		private void ModuleOnItemUpdated(ViewModelEventArgs<LoginViewModel> args)
		{
			this.Module.Update(this.Logins, args.ViewModel, this.TextSearch, this.SearchOption);
		}

		private void ModuleOnItemDeleted(ViewModelEventArgs<LoginViewModel> args)
		{
			this.Module.Delete(this.Logins, args.ViewModel);
		}

		private void DisplayLogins(LoginViewModel[] viewModels, Feature feature)
		{


			this.Module.SetupViewModels(viewModels);
			this.ApplySearch();
		}

		private void SearchByText() => this.ApplySearch();

		private void SearchByOption() => this.ApplySearch();

		private void ApplySearch()
		{
			var viewModels = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Logins.Clear();
			foreach (var viewModel in viewModels)
			{
				this.Logins.Add(viewModel);
			}
		}

		private void SortBy()
		{
			var index = 0;
			foreach (var viewModel in this.Module.Sort(this.Logins, this.SortOption))
			{
				this.Logins[index++] = viewModel;
			}
		}

		private Feature AddProgressDisplay(Feature feature)
		{
			feature.Started = _ => { this.IsWorking = true; };
			feature.Stopped = _ => { this.IsWorking = false; };

			return feature;
		}
	}
}