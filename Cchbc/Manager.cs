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
	public abstract class Manager<T, TViewItem> where T : IDbObject where TViewItem : ViewItem<T>
	{
		public List<TViewItem> ViewItems { get; } = new List<TViewItem>();

		public event EventHandler<FeatureEventArgs> OperationStart;
		private void OnOperationStart(FeatureEventArgs e)
		{
			e.Feature.StartMeasure();
			OperationStart?.Invoke(this, e);
		}

		public event EventHandler<FeatureEventArgs> OperationEnd;
		private void OnOperationEnd(FeatureEventArgs e)
		{
			e.Feature.StopMeasure();
			OperationEnd?.Invoke(this, e);
		}

		public event EventHandler<FeatureEventArgs> OperationError;
		private void OnOperationError(FeatureEventArgs e)
		{
			OperationError?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewItem>> ItemInserted;
		private void OnItemInserted(ObjectEventArgs<TViewItem> e)
		{
			ItemInserted?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewItem>> ItemUpdated;
		private void OnItemUpdated(ObjectEventArgs<TViewItem> e)
		{
			ItemUpdated?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewItem>> ItemDeleted;
		private void OnItemDeleted(ObjectEventArgs<TViewItem> e)
		{
			ItemDeleted?.Invoke(this, e);
		}

		private IModifiableAdapter<T> Adapter { get; }
		public Sorter<TViewItem> Sorter { get; }
		public Searcher<TViewItem> Searcher { get; }
		public FilterOption<TViewItem>[] FilterOptions { get; set; }

		protected Manager(IModifiableAdapter<T> adapter, Sorter<TViewItem> sorter, Searcher<TViewItem> searcher, FilterOption<TViewItem>[] filterOptions = null)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Adapter = adapter;
			this.Sorter = sorter;
			this.Searcher = searcher;
			this.FilterOptions = filterOptions;
		}

		public void SetupData(IEnumerable<TViewItem> viewItems)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));

			this.ViewItems.Clear();
			foreach (var viewItem in viewItems)
			{
				this.ViewItems.Add(viewItem);
			}
			this.Sorter.Sort(this.ViewItems, this.Sorter.CurrentOption);
		}

		public abstract ValidationResult[] ValidateProperties(TViewItem viewItem, Feature feature);

		public abstract Task<PermissionResult> CanInsertAsync(TViewItem viewItem, Feature feature);

		public abstract Task<PermissionResult> CanUpdateAsync(TViewItem viewItem, Feature feature);

		public abstract Task<PermissionResult> CanDeleteAsync(TViewItem viewItem, Feature feature);

		public Task InsertAsync(TViewItem viewItem, ModalDialog dialog, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewItem, dialog, feature, this.CanInsertAsync, this.InsertValidatedAsync);
		}

		public Task UpdateAsync(TViewItem viewItem, ModalDialog dialog, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewItem, dialog, feature, this.CanUpdateAsync, this.UpdateValidatedAsync);
		}

		public Task DeleteAsync(TViewItem viewItem, ModalDialog dialog, Feature feature)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			return ExecuteAsync(viewItem, dialog, feature, this.CanDeleteAsync, this.DeleteValidatedAsync);
		}

		public async Task ExecuteAsync(TViewItem viewItem, ModalDialog dialog, Feature feature, Func<TViewItem, Feature, Task<PermissionResult>> verifier, Func<TViewItem, FeatureEventArgs, Task> performer)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (verifier == null) throw new ArgumentNullException(nameof(verifier));
			if (performer == null) throw new ArgumentNullException(nameof(performer));

			var args = new FeatureEventArgs(feature);

			this.OnOperationStart(args);

			try
			{
				var permissionResult = await GetPermissionResult(viewItem, feature, verifier);
				if (permissionResult != null)
				{
					switch (permissionResult.Status)
					{
						case PermissionStatus.Allow:
							await performer(viewItem, args);
							break;
						case PermissionStatus.Confirm:
							this.SetupDialog(dialog, args);
							dialog.AcceptAction = async () => await performer(viewItem, args);
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

		public void Insert(ObservableCollection<TViewItem> viewItems, TViewItem viewItem, string textSearch, SearchOption<TViewItem> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var results = this.Search(textSearch, searchOption, new List<TViewItem>(1) { viewItem });
			if (results.Any())
			{
				viewItems.Insert(this.FindNewIndex(viewItems, viewItem), viewItem);

				this.Searcher.SetupCounts(viewItems);
			}
		}

		public void Update(ObservableCollection<TViewItem> viewItems, TViewItem viewItem, string textSearch, SearchOption<TViewItem> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var newIndex = -1;

			var results = this.Search(textSearch, searchOption, new List<TViewItem>(1) { viewItem });
			if (results.Any())
			{
				// Find the new index
				newIndex = this.FindNewIndex(viewItems, viewItem);
			}

			if (newIndex >= 0)
			{
				// Find the old index before insert
				var oldIndex = viewItems.IndexOf(viewItem);
				if (oldIndex != newIndex)
				{
					var tmp = viewItems[oldIndex];
					viewItems[oldIndex] = viewItem;
					viewItems[newIndex] = tmp;
				}
			}

			this.Searcher.SetupCounts(viewItems);
		}

		public void Delete(ObservableCollection<TViewItem> viewItems, TViewItem viewItem)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			viewItems.Remove(viewItem);

			// Only recalculate filter counts. Delete doesn't affect sort
			this.Searcher.SetupCounts(viewItems);
		}

		public IEnumerable<TViewItem> Sort(ICollection<TViewItem> currenTiewItems, SortOption<TViewItem> sortOption)
		{
			if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

			var flag = sortOption.Ascending ?? true;

			// Sort view items
			this.Sorter.Sort(this.ViewItems, sortOption);

			// Sort current view items
			var copy = new TViewItem[currenTiewItems.Count];
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
			foreach (var viewItem in copy)
			{
				yield return viewItem;
			}
		}

		public IEnumerable<TViewItem> Search(string textSearch, SearchOption<TViewItem> searchOption, List<TViewItem> viewItems = null)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			return this.Searcher.Search(GetFilteredViewItems(viewItems ?? this.ViewItems), textSearch, searchOption);
		}

		public async Task InsertValidatedAsync(TViewItem viewItem, FeatureEventArgs args)
		{
			var feature = args.Feature;
			feature.AddStep(nameof(InsertValidatedAsync));
			try
			{
				try
				{
					// Add the item to the db
					await this.Adapter.InsertAsync(viewItem.Item);

					// Find the right index to insert the new element
					var index = this.ViewItems.Count;
					var option = this.Sorter.CurrentOption;
					if (option != null)
					{
						index = 0;

						var cmp = option.Comparison;
						foreach (var current in this.ViewItems)
						{
							var result = cmp(current, viewItem);
							if (result > 0)
							{
								break;
							}
							index++;
						}
					}

					// Insert the item into the list at the correct place
					this.ViewItems.Insert(index, viewItem);

					this.OnItemInserted(new ObjectEventArgs<TViewItem>(viewItem));
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

		public async Task UpdateValidatedAsync(TViewItem viewItem, FeatureEventArgs args)
		{
			try
			{
				// Update the item from the db
				await this.Adapter.UpdateAsync(viewItem.Item);

				this.OnItemUpdated(new ObjectEventArgs<TViewItem>(viewItem));
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

		public async Task DeleteValidatedAsync(TViewItem viewItem, FeatureEventArgs args)
		{
			try
			{
				// Delete the item from the db
				await this.Adapter.DeleteAsync(viewItem.Item);

				// Delete the item into the list at the correct place
				this.ViewItems.Remove(viewItem);

				this.OnItemDeleted(new ObjectEventArgs<TViewItem>(viewItem));
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

		private int FindNewIndex(ObservableCollection<TViewItem> viewItems, TViewItem viewItem)
		{
			// At the end by default
			var index = viewItems.Count;

			var option = this.Sorter.CurrentOption;
			if (option != null)
			{
				index = 0;

				// Insert at the right place according to the current sort option
				var comparison = option.Comparison;
				foreach (var item in viewItems)
				{
					var cmp = comparison(item, viewItem);
					if (cmp > 0)
					{
						break;
					}
					index++;
				}
			}

			return index;
		}

		private ICollection<TViewItem> GetFilteredViewItems(ICollection<TViewItem> viewItems)
		{
			if (this.FilterOptions != null && this.FilterOptions.Length > 0)
			{
				viewItems = new List<TViewItem>();

				foreach (var item in this.ViewItems)
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
						viewItems.Add(item);
					}
				}
			}

			return viewItems;
		}

		private async Task<PermissionResult> GetPermissionResult(TViewItem viewItem, Feature feature, Func<TViewItem, Feature, Task<PermissionResult>> checker)
		{
			PermissionResult permissionResult = null;

			// Validate properties
			var validationResults = this.ValidateProperties(viewItem, feature);
			if (validationResults.Length == 0)
			{
				// Apply business logic
				permissionResult = await checker(viewItem, feature);
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