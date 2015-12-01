using System;
using System.Threading.Tasks;
using Cchbc.Features;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.UI
{
	public sealed class LoginModule : Module<Login, LoginViewModel>
	{
		public LoginAdapter Adapter { get; }

		public LoginModule(LoginAdapter adapter, Sorter<LoginViewModel> sorter, Searcher<LoginViewModel> searcher, FilterOption<LoginViewModel>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public override ValidationResult[] ValidateProperties(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(ValidateProperties));
			try
			{
				return new[] { ValidationResult.Success };
			}
			finally
			{
				feature.EndStep();
			}
		}

		public override Task<PermissionResult> CanInsertAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanInsertAsync));
			try
			{
				return Task.FromResult(PermissionResult.Deny(string.Empty));
			}
			finally
			{
				feature.EndStep();
			}
		}

		public override Task<PermissionResult> CanUpdateAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanUpdateAsync));
			try
			{
				return Task.FromResult(PermissionResult.Deny(string.Empty));
			}
			finally
			{
				feature.EndStep();
			}
		}

		public override Task<PermissionResult> CanDeleteAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanDeleteAsync));
			try
			{
				return Task.FromResult(PermissionResult.Deny(string.Empty));
			}
			finally
			{
				feature.EndStep();
			}
		}
	}
}