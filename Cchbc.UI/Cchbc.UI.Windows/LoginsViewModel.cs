using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Cchbc.Common;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI
{
	public sealed class LoginsViewModel : ViewModel
	{
		private IModalDialog ModalDialog { get; }
		private FeatureManager FeatureManager { get; }
		private LoginsModule Module { get; }
		private string Context { get; } = nameof(LoginsViewModel);

		public ObservableCollection<LoginViewModel> Logins { get; } = new ObservableCollection<LoginViewModel>();
		public SortOption<LoginViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<LoginViewModel>[] SearchOptions => this.Module.Searcher.Options;

		public ICommand AddCommand { get; }

		private bool _isWorking;
		public bool IsWorking
		{
			get { return _isWorking; }
			private set { this.SetField(ref _isWorking, value); }
		}

		private string _workProgress;
		public string WorkProgress
		{
			get { return _workProgress; }
			private set { this.SetField(ref _workProgress, value); }
		}

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);

				var feature = this.FeatureManager.StartNew(this.Context, nameof(SearchByText));
				this.SearchByText();
				this.FeatureManager.Stop(feature);
			}
		}

		private SearchOption<LoginViewModel> _searchOption;
		public SearchOption<LoginViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				var feature = this.FeatureManager.StartNew(this.Context, nameof(SearchByOption));
				this.SearchByOption();
				this.FeatureManager.Stop(feature, value?.Name ?? string.Empty);
			}
		}

		private SortOption<LoginViewModel> _sortOption;
		public SortOption<LoginViewModel> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				var feature = this.FeatureManager.StartNew(this.Context, nameof(SortBy));
				this.SortBy();
				this.FeatureManager.Stop(feature);
			}
		}

		public LoginsViewModel(Core core, LoginAdapter adapter)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.ModalDialog = core.ModalDialog;
			this.FeatureManager = core.FeatureManager;
			this.Module = new LoginsModule(adapter, new Sorter<LoginViewModel>(new[]
			{
				new SortOption<LoginViewModel>(string.Empty, (x, y) => 0)
			}), new Searcher<LoginViewModel>((v, s) => false));

			this.Module.OperationStart += (sender, args) =>
			{
				this.FeatureManager.Start(args.Feature);
			};
			this.Module.OperationEnd += (sender, args) =>
			{
				this.FeatureManager.Stop(args.Feature);
			};
			this.Module.OperationError += (sender, args) =>
			{
				this.FeatureManager.LogException(args.Feature, args.Exception);
			};

			this.Module.ItemInserted += ModuleOnItemInserted;
			this.Module.ItemUpdated += ModuleOnItemUpdated;
			this.Module.ItemDeleted += ModuleOnItemDeleted;

			this.AddCommand = new RelayCommand(this.Add);
		}

		private string _name = string.Empty;
		public string Name
		{
			get { return _name; }
			set { this.SetField(ref _name, value); }
		}

		private string _password = string.Empty;
		public string Password
		{
			get { return _password; }
			set { this.SetField(ref _password, value); }
		}

		private bool _isSystem;
		public bool IsSystem
		{
			get { return _isSystem; }
			set { this.SetField(ref _isSystem, value); }
		}

		private async void Add()
		{
			var feature = new Feature(this.Context, nameof(this.Add));
			try
			{
				var login = new Login(0, this.Name, this.Password, DateTime.Now, this.IsSystem);
				var viewModel = new LoginViewModel(login);

				await this.Module.InsertAsync(viewModel, this.ModalDialog, this.AddProgressDisplay(feature));
			}
			catch (Exception ex)
			{
				this.FeatureManager.LogException(feature, ex);
			}
		}

		public void LoadData()
		{
			var feature = this.FeatureManager.StartNew(this.Context, nameof(LoadData));

			try
			{
				var models = this.Module.GetAll();
				var viewModels = new LoginViewModel[models.Count];
				var index = 0;
				foreach (var model in models)
				{
					viewModels[index++] = new LoginViewModel(model);
				}
				this.DisplayLogins(viewModels, feature);

				this.FeatureManager.Stop(feature, models.Count.ToString());
			}
			catch (Exception ex)
			{
				this.FeatureManager.LogException(feature, ex);
			}
		}

		public Task UpdateAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.UpdateAsync(viewModel, dialog, this.AddProgressDisplay(new Feature(this.Context, nameof(UpdateAsync))));
		}

		public Task DeleteAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.DeleteAsync(viewModel, dialog, this.AddProgressDisplay(new Feature(this.Context, nameof(DeleteAsync))));
		}

		private void ModuleOnItemInserted(object sender, ObjectEventArgs<LoginViewModel> args)
		{
			this.Module.Insert(this.Logins, args.ViewModel, this.TextSearch, this.SearchOption);
		}

		private void ModuleOnItemUpdated(object sender, ObjectEventArgs<LoginViewModel> args)
		{
			this.Module.Update(this.Logins, args.ViewModel, this.TextSearch, this.SearchOption);
		}

		private void ModuleOnItemDeleted(object sender, ObjectEventArgs<LoginViewModel> args)
		{
			this.Module.Delete(this.Logins, args.ViewModel);
		}

		private void DisplayLogins(LoginViewModel[] viewModels, Feature feature)
		{
			feature.AddStep(nameof(DisplayLogins));

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
			feature.StepAdded = f => this.WorkProgress = f.Step.Name;

			return feature;
		}
	}
}