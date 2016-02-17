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
	public abstract class Module<T, TViewModel>
		where T : class, IDbObject
		where TViewModel : ViewModel<T>
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

		public void SetupViewModels(TViewModel[] viewModels)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));

			this.ViewModels.Clear();
			this.ViewModels.AddRange(viewModels);

			this.Sorter.Sort(this.ViewModels, this.Sorter.CurrentOption);
		}

		public TViewModel FindViewModel(T model)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));

			foreach (var viewModel in this.ViewModels)
			{
				if (viewModel.Model == model)
				{
					return viewModel;
				}
			}

			return null;
		}

		public abstract ValidationResult[] ValidateProperties(TViewModel viewModel, Feature feature);

		public abstract Task<PermissionResult> CanInsertAsync(TViewModel viewModel, Feature feature);

		public abstract Task<PermissionResult> CanUpdateAsync(TViewModel viewModel, Feature feature);

		public abstract Task<PermissionResult> CanDeleteAsync(TViewModel viewModel, Feature feature);

		public Task InsertAsync(TViewModel viewModel, IModalDialog dialog, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewModel, dialog, feature, this.CanInsertAsync, this.InsertValidatedAsync);
		}

		public Task UpdateAsync(TViewModel viewModel, IModalDialog dialog, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewModel, dialog, feature, this.CanUpdateAsync, this.UpdateValidatedAsync);
		}

		public Task DeleteAsync(TViewModel viewModel, IModalDialog dialog, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewModel, dialog, feature, this.CanDeleteAsync, this.DeleteValidatedAsync);
		}

		public async Task ExecuteAsync(TViewModel viewModel, IModalDialog dialog, Feature feature, Func<TViewModel, Feature, Task<PermissionResult>> checker, Func<TViewModel, FeatureEventArgs, Task> performer)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (checker == null) throw new ArgumentNullException(nameof(checker));
			if (performer == null) throw new ArgumentNullException(nameof(performer));

			var args = new FeatureEventArgs(feature);
			try
			{
				this.OnOperationStart(args);

				// Validate properties
				var validationResults = this.ValidateProperties(viewModel, feature);
				if (validationResults.Length == 0)
				{
					// Apply business logic
					var permissionResult = await checker(viewModel, feature);
					if (permissionResult != null)
					{
						switch (permissionResult.Status)
						{
							case PermissionStatus.Allow:
								await performer(viewModel, args);
								break;
							case PermissionStatus.Confirm:
								var dialogResult = await dialog.ShowAsync(permissionResult.Message, feature, DialogType.AcceptDecline);
								if (dialogResult == DialogResult.Accept)
								{
									await performer(viewModel, args);
								}
								break;
							case PermissionStatus.Deny:
								await dialog.ShowAsync(permissionResult.Message, feature);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
				}
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

		public async Task InsertValidatedAsync(TViewModel viewModel, FeatureEventArgs args)
		{
			args.Feature.AddStep(nameof(InsertValidatedAsync));

			// Add the item to the db
			await this.Adapter.InsertAsync(viewModel.Model);

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

		public async Task UpdateValidatedAsync(TViewModel viewModel, FeatureEventArgs args)
		{
			args.Feature.AddStep(nameof(UpdateValidatedAsync));

			// Update the item from the db
			await this.Adapter.UpdateAsync(viewModel.Model);

			// Fire the event
			this.OnItemUpdated(new ObjectEventArgs<TViewModel>(viewModel));
		}

		public async Task DeleteValidatedAsync(TViewModel viewModel, FeatureEventArgs args)
		{
			args.Feature.AddStep(nameof(UpdateValidatedAsync));

			// Delete the item from the db
			await this.Adapter.DeleteAsync(viewModel.Model);

			// Delete the item from the list
			this.ViewModels.Remove(viewModel);

			// Fire the event
			this.OnItemDeleted(new ObjectEventArgs<TViewModel>(viewModel));
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
	}
}