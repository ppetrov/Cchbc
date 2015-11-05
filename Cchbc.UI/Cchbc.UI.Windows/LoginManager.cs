﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Features;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.UI
{
	public sealed class LoginManager : Manager<Login, LoginViewItem>
	{
		public ILogger Logger { get; }
		public LoginAdapter Adapter { get; }

		public LoginManager(ILogger logger, LoginAdapter adapter, Sorter<LoginViewItem> sorter, Searcher<LoginViewItem> searcher, FilterOption<LoginViewItem>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Logger = logger;
			this.Adapter = adapter;
		}

		public override ValidationResult[] ValidateProperties(LoginViewItem viewItem, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(ValidateProperties));

			try
			{
				return Validator.GetViolated(new[]
				{
					Validator.ValidateNotNull(viewItem.Name, @"Name cannot be null"),
					Validator.ValidateNotEmpty(viewItem.Name, @"Name cannot be empty"),
					Validator.ValidateMaxLength(viewItem.Name, 8, @"Name cannot be more then 8"),

					Validator.ValidateNotNull(viewItem.Password, @"Password cannot be null"),
					Validator.ValidateNotEmpty(viewItem.Password, @"Password cannot be empty"),
					Validator.ValidateMinLength(viewItem.Password, 8, @"Password is too short. Must be at least 8 symbols"),
					Validator.ValidateMaxLength(viewItem.Password, 20, @"Password is too long. Must be less then or equal to 20")
				});
			}
			finally
			{
				feature.EndStep();
			}
		}

		public override Task<PermissionResult> CanInsertAsync(LoginViewItem viewItem, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanInsertAsync));
			try
			{
				//if (await this.Adapter.IsReservedAsync(viewItem.Name))
				//{
				//	return PermissionResult.Deny(@"This name is reserved");
				//}
				//return PermissionResult.Deny(@"The name is reserved");
				//return PermissionResult.Confirm(@"Are you sure ???");
				return Task.FromResult(PermissionResult.Allow);
			}
			finally
			{
				feature.EndStep();
			}
		}

		public override Task<PermissionResult> CanUpdateAsync(LoginViewItem viewItem, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanUpdateAsync));
			try
			{
				return Task.FromResult(PermissionResult.Allow);
			}
			finally
			{
				feature.EndStep();
			}
		}

		public override Task<PermissionResult> CanDeleteAsync(LoginViewItem viewItem, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanDeleteAsync));
			try
			{
				if (viewItem.Item.CreatedAt.Date == DateTime.Today)
				{
					return Task.FromResult(PermissionResult.Confirm(@"Cannot delete today logins"));
				}
				return Task.FromResult(PermissionResult.Allow);
			}
			finally
			{
				feature.EndStep();
			}
		}

		public Task<PermissionResult> CanPromoteAsync(LoginViewItem viewItem, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			var s = Stopwatch.StartNew();
			try
			{
				return Task.FromResult(PermissionResult.Allow);
				if (viewItem.IsSystem)
				{
					return Task.FromResult(PermissionResult.Deny(@"The user is already System user"));
				}
				return Task.FromResult(PermissionResult.Confirm(@"Are you sure to promoto user create today ?"));
			}
			finally
			{
				this.Logger.Info($@"{nameof(CanPromoteAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}
	}
}