using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Features;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.UI
{
	public sealed class LoginsModule : Module<Login, LoginViewModel>
	{
		private LoginAdapter Adapter { get; }

		public LoginsModule(LoginAdapter adapter, Sorter<LoginViewModel> sorter, Searcher<LoginViewModel> searcher, FilterOption<LoginViewModel>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public List<Login> GetAll()
		{
			return this.Adapter.GetAll();
		}

		public override ValidationResult[] ValidateProperties(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Validate the model properties
			var model = viewModel.Model;

			//model.Name
			//model.Password

			feature.AddStep(nameof(ValidateProperties));
			return Enumerable.Empty<ValidationResult>().ToArray();
		}

		public override Task<PermissionResult> CanInsertAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanInsertAsync));
			return Task.FromResult(PermissionResult.Confirm(@"Add one more admin ?"));
		}

		public override Task<PermissionResult> CanUpdateAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanUpdateAsync));
			return Task.FromResult(PermissionResult.Deny(string.Empty));
		}

		public override Task<PermissionResult> CanDeleteAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanDeleteAsync));
			return Task.FromResult(PermissionResult.Deny(string.Empty));
		}

		public Task<PermissionResult> CanChangePassword(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanChangePassword));
			return PermissionResult.Allow;
		}
	}
}