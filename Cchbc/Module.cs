using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc
{
	public abstract class Module<T, TViewModel> where T : IDbObject where TViewModel : ViewModel<T>
	{
		public List<TViewModel> ViewModels { get; } = new List<TViewModel>();

		public event EventHandler<FeatureEventArgs> OperationStart;
		private void OnOperationStart(FeatureEventArgs e)
		{
			OperationStart?.Invoke(this, e);
		}

		public event EventHandler<FeatureEventArgs> OperationEnd;
		private void OnOperationEnd(FeatureEventArgs e)
		{
			OperationEnd?.Invoke(this, e);
		}

		public event EventHandler<FeatureEventArgs> OperationError;
		private void OnOperationError(FeatureEventArgs e)
		{
			OperationError?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewModel>> ItemInserted;
		private void OnItemInserted(ObjectEventArgs<TViewModel> e)
		{
			ItemInserted?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewModel>> ItemUpdated;
		private void OnItemUpdated(ObjectEventArgs<TViewModel> e)
		{
			ItemUpdated?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewModel>> ItemDeleted;
		private void OnItemDeleted(ObjectEventArgs<TViewModel> e)
		{
			ItemDeleted?.Invoke(this, e);
		}

		private IModifiableAdapter<T> Adapter { get; }
		public Sorter<TViewModel> Sorter { get; }
		public Searcher<TViewModel> Searcher { get; }
		public FilterOption<TViewModel>[] FilterOptions { get; set; }

		protected Module(IModifiableAdapter<T> adapter, Sorter<TViewModel> sorter, Searcher<TViewModel> searcher, FilterOption<TViewModel>[] filterOptions = null)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Adapter = adapter;
			this.Sorter = sorter;
			this.Searcher = searcher;
			this.FilterOptions = filterOptions;
		}

		public void SetupData(IEnumerable<TViewModel> viewModels)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));

			this.ViewModels.Clear();
			foreach (var viewModel in viewModels)
			{
				this.ViewModels.Add(viewModel);
			}
			this.Sorter.Sort(this.ViewModels, this.Sorter.CurrentOption);
		}

		public abstract ValidationResult[] ValidateProperties(TViewModel viewModel, Feature feature);

		public abstract Task<PermissionResult> CanInsertAsync(TViewModel viewModel, Feature feature);

		public abstract Task<PermissionResult> CanUpdateAsync(TViewModel viewModel, Feature feature);

		public abstract Task<PermissionResult> CanDeleteAsync(TViewModel viewModel, Feature feature);

		public Task InsertAsync(TViewModel viewModel, ModalDialog dialog, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewModel, dialog, feature, this.CanInsertAsync, this.InsertValidated);
		}

		public Task UpdateAsync(TViewModel viewModel, ModalDialog dialog, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewModel, dialog, feature, this.CanUpdateAsync, this.UpdateValidated);
		}

		public Task DeleteAsync(TViewModel viewModel, ModalDialog dialog, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewModel, dialog, feature, this.CanDeleteAsync, this.DeleteValidated);
		}

		public async Task ExecuteAsync(TViewModel viewModel, ModalDialog dialog, Feature feature, Func<TViewModel, Feature, Task<PermissionResult>> verifier, Action<TViewModel, FeatureEventArgs> performer)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (verifier == null) throw new ArgumentNullException(nameof(verifier));
			if (performer == null) throw new ArgumentNullException(nameof(performer));

			var args = new FeatureEventArgs(feature);

			this.OnOperationStart(args);

			try
			{
				var permissionResult = await GetPermissionResultAsync(viewModel, feature, verifier);
				if (permissionResult != null)
				{
					switch (permissionResult.Status)
					{
						case PermissionStatus.Allow:
							performer(viewModel, args);
							break;
						case PermissionStatus.Confirm:
							this.SetupDialog(dialog, args);
							dialog.AcceptAction = () => performer(viewModel, args);
							await dialog.ConfirmAsync(permissionResult.Message, feature);
							break;
						case PermissionStatus.Deny:
							this.SetupDialog(dialog, args);
							await dialog.DisplayAsync(permissionResult.Message, feature);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception ex)
			{
				this.OnOperationError(args.WithException(ex));
				this.OnOperationEnd(args);
			}
		}

		public void Insert(ObservableCollection<TViewModel> viewModels, TViewModel viewModel, string textSearch, SearchOption<TViewModel> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var results = this.Search(textSearch, searchOption, new List<TViewModel>(1) { viewModel });
			if (results.Any())
			{
				viewModels.Insert(this.FindNewIndex(viewModels, viewModel), viewModel);

				this.Searcher.SetupCounts(viewModels);
			}
		}

		public void Update(ObservableCollection<TViewModel> viewModels, TViewModel viewModel, string textSearch, SearchOption<TViewModel> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var newIndex = -1;

			var results = this.Search(textSearch, searchOption, new List<TViewModel>(1) { viewModel });
			if (results.Any())
			{
				// Find the new index
				newIndex = this.FindNewIndex(viewModels, viewModel);

				// New index can be at the end
				newIndex = Math.Min(newIndex, viewModels.Count - 1);
			}

			if (newIndex >= 0)
			{
				// Find the old index before insert
				var oldIndex = viewModels.IndexOf(viewModel);
				if (oldIndex != newIndex)
				{
					var tmp = viewModels[newIndex];
					viewModels[newIndex] = viewModel;
					viewModels[oldIndex] = tmp;
				}
			}

			this.Searcher.SetupCounts(viewModels);
		}

		public void Delete(ObservableCollection<TViewModel> viewModels, TViewModel viewModel)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			viewModels.Remove(viewModel);

			// Only recalculate filter counts. Delete doesn't affect sort
			this.Searcher.SetupCounts(viewModels);
		}

		public IEnumerable<TViewModel> Sort(ICollection<TViewModel> currenTiewItems, SortOption<TViewModel> sortOption)
		{
			if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

			var flag = sortOption.Ascending ?? true;

			// Sort view items
			this.Sorter.Sort(this.ViewModels, sortOption);

			// Sort current view items
			var copy = new TViewModel[currenTiewItems.Count];
			currenTiewItems.CopyTo(copy, 0);
			this.Sorter.Sort(copy, sortOption);

			// Set the new flag
			if (sortOption.Ascending.HasValue)
			{
				sortOption.Ascending = !flag;
			}
			else
			{
				sortOption.Ascending = true;
			}

			// Clear all sort options
			foreach (var option in this.Sorter.Options)
			{
				if (option != sortOption)
				{
					option.Ascending = null;
				}
			}

			// Return current view items sorted
			foreach (var viewModel in copy)
			{
				yield return viewModel;
			}
		}

		public IEnumerable<TViewModel> Search(string textSearch, SearchOption<TViewModel> searchOption, List<TViewModel> viewModels = null)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			return this.Searcher.Search(GetFilteredViewModels(viewModels ?? this.ViewModels), textSearch, searchOption);
		}

		public void InsertValidated(TViewModel viewModel, FeatureEventArgs args)
		{
			var feature = args.Feature;
			feature.AddStep(nameof(InsertValidated));
			try
			{
				try
				{
					// Add the item to the db
					this.Adapter.Insert(viewModel.Model);

					// Find the right index to insert the new element
					var index = this.ViewModels.Count;
					var option = this.Sorter.CurrentOption;
					if (option != null)
					{
						index = 0;

						var cmp = option.Comparison;
						foreach (var current in this.ViewModels)
						{
							var result = cmp(current, viewModel);
							if (result > 0)
							{
								break;
							}
							index++;
						}
					}

					// Insert the item into the list at the correct place
					this.ViewModels.Insert(index, viewModel);

					// Fire the event
					this.OnItemInserted(new ObjectEventArgs<TViewModel>(viewModel));
				}
				catch (Exception ex)
				{
					this.OnOperationError(args.WithException(ex));
				}
				finally
				{
					this.OnOperationEnd(args);
				}
			}
			finally
			{
				feature.EndStep();
			}
		}

		public void UpdateValidated(TViewModel viewModel, FeatureEventArgs args)
		{
			var feature = args.Feature;
			feature.AddStep(nameof(UpdateValidated));
			try
			{
				try
				{
					// Update the item from the db
					this.Adapter.Update(viewModel.Model);

					// Fire the event
					this.OnItemUpdated(new ObjectEventArgs<TViewModel>(viewModel));
				}
				catch (Exception ex)
				{
					this.OnOperationError(args.WithException(ex));
				}
				finally
				{
					this.OnOperationEnd(args);
				}
			}
			finally
			{
				feature.EndStep();
			}
		}

		public void DeleteValidated(TViewModel viewModel, FeatureEventArgs args)
		{
			var feature = args.Feature;
			feature.AddStep(nameof(UpdateValidated));
			try
			{
				try
				{
					// Delete the item from the db
					this.Adapter.Delete(viewModel.Model);

					// Delete the item into the list at the correct place
					this.ViewModels.Remove(viewModel);

					// Fire the event
					this.OnItemDeleted(new ObjectEventArgs<TViewModel>(viewModel));
				}
				catch (Exception ex)
				{
					this.OnOperationError(args.WithException(ex));
				}
				finally
				{
					this.OnOperationEnd(args);
				}
			}
			finally
			{
				feature.EndStep();
			}
		}

		private int FindNewIndex(ObservableCollection<TViewModel> viewModels, TViewModel viewModel)
		{
			// At the end by default
			var index = viewModels.Count;

			var option = this.Sorter.CurrentOption;
			if (option != null)
			{
				index = 0;

				// Insert at the right place according to the current sort option
				var comparison = option.Comparison;
				foreach (var item in viewModels)
				{
					var cmp = comparison(item, viewModel);
					if (cmp > 0)
					{
						break;
					}
					index++;
				}
			}

			return index;
		}

		private ICollection<TViewModel> GetFilteredViewModels(ICollection<TViewModel> viewModels)
		{
			if (this.FilterOptions != null && this.FilterOptions.Length > 0)
			{
				viewModels = new List<TViewModel>();

				foreach (var item in this.ViewModels)
				{
					var include = true;

					foreach (var filter in this.FilterOptions)
					{
						if (filter.IsSelected)
						{
							include &= filter.IsMatch(item);
							if (!include)
							{
								break;
							}
						}
					}

					if (include)
					{
						viewModels.Add(item);
					}
				}
			}

			return viewModels;
		}

		private async Task<PermissionResult> GetPermissionResultAsync(TViewModel viewModel, Feature feature, Func<TViewModel, Feature, Task<PermissionResult>> checker)
		{
			PermissionResult permissionResult = null;

			// Validate properties
			var validationResults = this.ValidateProperties(viewModel, feature);
			if (validationResults.Length == 0)
			{
				// Apply business logic
				permissionResult = await checker(viewModel, feature);
			}

			return permissionResult;
		}

		private void SetupDialog(ModalDialog dialog, FeatureEventArgs args)
		{
			// On every action of the dialog fire End event for None feature - works like finally
			dialog.AcceptAction =
				dialog.CancelAction = dialog.DeclineAction = () => this.OnOperationEnd(args);
		}
	}
}