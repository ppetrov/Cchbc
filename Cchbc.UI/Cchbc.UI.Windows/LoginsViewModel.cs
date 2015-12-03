using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI
{
	public sealed class LoginsViewModel : ViewModel
	{
		private Core Core { get; }
		private FeatureManager FeatureManager => this.Core.FeatureManager;
		private LoginModule Module { get; }
		private string Context { get; } = nameof(LoginsViewModel);

		public ObservableCollection<LoginViewModel> Logins { get; } = new ObservableCollection<LoginViewModel>();
		public SortOption<LoginViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<LoginViewModel>[] SearchOptions => this.Module.Searcher.Options;

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);
				this.ApplySearch();
			}
		}

		private SearchOption<LoginViewModel> _searchOption;
		public SearchOption<LoginViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				this.ApplySearch();
			}
		}

		private SortOption<LoginViewModel> _sortOption;
		public SortOption<LoginViewModel> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				this.ApplySort();
			}
		}

		public LoginsViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			this.Core = core;
			this.Module = new LoginModule(new LoginAdapter(), new Sorter<LoginViewModel>(new[]
			{
				new SortOption<LoginViewModel>(string.Empty, (x, y) => 0)
			}),
			new Searcher<LoginViewModel>((v, s) => false));

			this.Module.OperationStart += (sender, args) =>
			{
				this.Core.FeatureManager.Start(args.Feature);
			};
			this.Module.OperationEnd += (sender, args) =>
			{
				this.Core.FeatureManager.Stop(args.Feature);
			};
			this.Module.OperationError += (sender, args) =>
			{
				this.Core.FeatureManager.LogException(args.Feature, args.Exception);
			};

			this.Module.ItemInserted += ModuleOnItemInserted;
			this.Module.ItemUpdated += ModuleOnItemUpdated;
			this.Module.ItemDeleted += ModuleOnItemDeleted;
		}

		public void LoadData()
		{
			var feature = Feature.StartNew(this.Context, nameof(LoadData));

			var models = this.Module.Adapter.GetAll();
			var viewModels = new LoginViewModel[models.Count];
			var index = 0;
			foreach (var model in models)
			{
				viewModels[index++] = new LoginViewModel(model);
			}
			this.DisplayLogins(viewModels, feature);

			this.FeatureManager.Stop(feature);
		}

		public Task AddAsync(LoginViewModel viewModel, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.InsertAsync(viewModel, dialog, new Feature(this.Context, nameof(AddAsync), string.Empty));
		}

		public Task UpdateAsync(LoginViewModel viewModel, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.UpdateAsync(viewModel, dialog, new Feature(this.Context, nameof(UpdateAsync), string.Empty));
		}

		public Task ChangePasswordAsync(LoginViewModel viewModel, ModalDialog dialog, string newPassword)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var copyViewModel = new LoginViewModel(viewModel.Model) { Password = newPassword };

			return this.Module.ExecuteAsync(copyViewModel, dialog, new Feature(this.Context, nameof(ChangePasswordAsync), string.Empty), this.Module.CanChangePassword, this.ChangePasswordValidated);
		}

		public Task RemoveAsync(LoginViewModel viewModel, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.DeleteAsync(viewModel, dialog, new Feature(this.Context, nameof(RemoveAsync), string.Empty));
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

		private void ApplySearch()
		{
			var viewModels = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Logins.Clear();
			foreach (var viewModel in viewModels)
			{
				this.Logins.Add(viewModel);
			}
		}

		private void ApplySort()
		{
			var index = 0;
			foreach (var viewModel in this.Module.Sort(this.Logins, this.SortOption))
			{
				this.Logins[index++] = viewModel;
			}
		}

		private void ChangePasswordValidated(LoginViewModel copyViewModel, FeatureEventArgs args)
		{
			var model = copyViewModel.Model;

			foreach (var viewModel in this.Module.ViewModels)
			{
				if (viewModel.Model == model)
				{
					viewModel.Password = copyViewModel.Password;

					this.Module.UpdateValidated(viewModel, args);
					break;
				}
			}
		}
	}
}