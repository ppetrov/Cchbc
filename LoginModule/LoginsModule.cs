using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;
using LoginModule.Adapter;
using LoginModule.Objects;
using LoginModule.ViewModels;

namespace LoginModule
{
	public sealed class LoginsModule : Module<Login, LoginViewModel>
	{
		private LoginAdapter Adapter { get; }

		public LoginsModule(ITransactionContextCreator contextCreator, LoginAdapter adapter, Sorter<LoginViewModel> sorter, Searcher<LoginViewModel> searcher,
			FilterOption<LoginViewModel>[] filterOptions = null)
			: base(contextCreator, adapter, sorter, searcher, filterOptions)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public Task<List<Login>> GetAllAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				return this.Adapter.GetAllAsync(context);
			}
		}

		public bool IsAvailable(LoginViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var name = (viewModel.Name ?? string.Empty).Trim();
			foreach (var model in this.ViewModels)
			{
				if (model.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			return true;
		}

		public override IEnumerable<ValidationResult> ValidateProperties(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(ValidateProperties));

			var model = viewModel.Model;
			var name = (model.Name ?? string.Empty).Trim();
			if (name == string.Empty)
			{
				yield return new ValidationResult(@"Name is required.", nameof(Login.Name));
			}
			var password = model.Password ?? string.Empty;
			if (password == string.Empty)
			{
				yield return new ValidationResult(@"Password is required.", nameof(Login.Password));
			}
			else
			{
				if (password.Length < 8)
				{
					yield return new ValidationResult(@"Password is too short.", nameof(Login.Password));
				}
			}
		}

		public override Task<PermissionResult> CanInsertAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanInsertAsync));

			if (!this.IsAvailable(viewModel))
			{
				return Task.FromResult(PermissionResult.Deny(@"Login with the same username already exists"));
			}

			return PermissionResult.Allow;
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