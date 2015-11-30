using System;
using System.Threading.Tasks;
using Cchbc.Features;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.UI
{
	public sealed class LoginManager : Manager<Login, LoginViewModel>
	{
		public ILogger Logger { get; }
		public LoginAdapter Adapter { get; }

		public LoginManager(ILogger logger, LoginAdapter adapter, Sorter<LoginViewModel> sorter, Searcher<LoginViewModel> searcher, FilterOption<LoginViewModel>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Logger = logger;
			this.Adapter = adapter;
		}

		public override ValidationResult[] ValidateProperties(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(ValidateProperties));

			try
			{
				return Validator.GetViolated(new[]
				{
					Validator.ValidateNotNull(viewModel.Name, @"Name cannot be null"),
					Validator.ValidateNotEmpty(viewModel.Name, @"Name cannot be empty"),
					Validator.ValidateMaxLength(viewModel.Name, 8, @"Name cannot be more then 8"),

					Validator.ValidateNotNull(viewModel.Password, @"Password cannot be null"),
					Validator.ValidateNotEmpty(viewModel.Password, @"Password cannot be empty"),
					Validator.ValidateMinLength(viewModel.Password, 8, @"Password is too short. Must be at least 8 symbols"),
					Validator.ValidateMaxLength(viewModel.Password, 20, @"Password is too long. Must be less then or equal to 20")
				});
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
				return PermissionResult.Allow;
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
				return PermissionResult.Allow;
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
				if (viewModel.Model.CreatedAt.Date == DateTime.Today)
				{
					return Task.FromResult(PermissionResult.Confirm(@"Cannot delete today logins"));
				}
				return PermissionResult.Allow;
			}
			finally
			{
				feature.EndStep();
			}
		}

		public Task<PermissionResult> CanPromoteAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanPromoteAsync));
			try
			{
				return PermissionResult.Allow;
			}
			finally
			{
				feature.EndStep();
			}
		}
	}
}