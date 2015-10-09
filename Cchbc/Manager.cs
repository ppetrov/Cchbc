using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc
{
	public abstract class Manager<T, TViewItem> where T : IDbObject where TViewItem : ViewItem<T>
	{
		protected List<TViewItem> ViewItems { get; } = new List<TViewItem>();

		public event EventHandler<ManagerOperationEventArgs> StartOperation;
		protected virtual void OnStartOperation(ManagerOperationEventArgs e)
		{
			StartOperation?.Invoke(this, e);
		}

		public event EventHandler<ManagerOperationEventArgs> EndOperation;
		protected virtual void OnEndOperation(ManagerOperationEventArgs e)
		{
			EndOperation?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewItem>> ItemInserted;
		protected virtual void OnItemInserted(ObjectEventArgs<TViewItem> e)
		{
			ItemInserted?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewItem>> ItemUpdated;
		protected virtual void OnItemUpdated(ObjectEventArgs<TViewItem> e)
		{
			ItemUpdated?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<TViewItem>> ItemDeleted;
		protected virtual void OnItemDeleted(ObjectEventArgs<TViewItem> e)
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

		public void LoadData(IEnumerable<TViewItem> viewItems)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));

			this.ViewItems.Clear();
			foreach (var viewItem in viewItems)
			{
				this.ViewItems.Add(viewItem);
			}
			this.Sorter.Sort(this.ViewItems, this.Sorter.CurrentOption);
		}

		public abstract ValidationResult[] ValidateProperties(TViewItem viewItem);

		public abstract Task<PermissionResult> CanAddAsync(TViewItem viewItem);

		public abstract Task<PermissionResult> CanUpdateAsync(TViewItem viewItem);

		public abstract Task<PermissionResult> CanDeleteAsync(TViewItem viewItem);

		public async Task AddAsync(TViewItem viewItem, ModalDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));

			var args = new ManagerOperationEventArgs(ManagerOperation.Add);
			this.OnStartOperation(args);

			var fireEndOperation = true;
			try
			{
				var validationResults = this.ValidateProperties(viewItem);
				if (validationResults.Length == 0)
				{
					// Apply business logic
					var permissionResult = await this.CanAddAsync(viewItem);
					switch (permissionResult.Status)
					{
						case PermissionStatus.Allow:
							await this.AddValidatedAsync(viewItem);
							break;
						case PermissionStatus.Confirm:
							fireEndOperation = false;

							// Confirm any user warnings
							dialog.AcceptAction = async () =>
							{
								try
								{
									await this.AddValidatedAsync(viewItem);
								}
								finally
								{
									this.OnEndOperation(args);
								}
							};
							dialog.CancelAction = () => this.OnEndOperation(args);
							dialog.DeclineAction = () => this.OnEndOperation(args);
							await dialog.ConfirmAsync(permissionResult.Message);
							break;
						case PermissionStatus.Deny:
							await dialog.DisplayAsync(permissionResult.Message);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			finally
			{
				if (fireEndOperation)
				{
					this.OnEndOperation(args);
				}
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
				// Insert at the end by default
				var index = viewItems.Count;

				var option = this.Sorter.CurrentOption;
				if (option != null)
				{
					index = 0;

					// Insert at the right place according to the current sort option
					var comparison = option.Comparison;
					foreach (var login in viewItems)
					{
						var cmp = comparison(login, viewItem);
						if (cmp > 0)
						{
							break;
						}
						index++;
					}
				}

				viewItems.Insert(index, viewItem);
			}
		}

		public async Task UpdateAsync(TViewItem viewItem, ModalDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));

			var args = new ManagerOperationEventArgs(ManagerOperation.Update);
			this.OnStartOperation(args);

			var fireEndOperation = true;
			try
			{
				var validationResults = this.ValidateProperties(viewItem);
				if (validationResults.Length == 0)
				{
					// Apply business logic
					var permissionResult = await this.CanUpdateAsync(viewItem);
					switch (permissionResult.Status)
					{
						case PermissionStatus.Allow:
							await this.UpdateValidatedAsync(viewItem);
							break;
						case PermissionStatus.Confirm:
							fireEndOperation = false;

							// Confirm any user warnings
							dialog.AcceptAction = async () =>
							{
								try
								{
									await this.UpdateValidatedAsync(viewItem);
								}
								finally
								{
									this.OnEndOperation(args);
								}
							};
							dialog.CancelAction = () => this.OnEndOperation(args);
							dialog.DeclineAction = () => this.OnEndOperation(args);
							await dialog.ConfirmAsync(permissionResult.Message);
							break;
						case PermissionStatus.Deny:
							await dialog.DisplayAsync(permissionResult.Message);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			finally
			{
				if (fireEndOperation)
				{
					this.OnEndOperation(args);
				}
			}

		}

		public void Update(ObservableCollection<TViewItem> viewItems, TViewItem viewItem, string textSearch, SearchOption<TViewItem> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			// This is still better then re-applying the filter & sorting the data
			this.Delete(viewItems, viewItem);
			this.Insert(viewItems, viewItem, textSearch, searchOption);
		}

		public async Task DeleteAsync(TViewItem viewItem, ModalDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));

			var args = new ManagerOperationEventArgs(ManagerOperation.Delete);
			this.OnStartOperation(args);

			var fireEndOperation = true;
			try
			{
				var permissionResult = await this.CanDeleteAsync(viewItem);
				switch (permissionResult.Status)
				{
					case PermissionStatus.Allow:
						await this.DeleteValidatedAsync(viewItem);
						break;
					case PermissionStatus.Confirm:
						fireEndOperation = false;

						// Confirm any user warnings
						dialog.AcceptAction = async () =>
						{
							try
							{
								await this.DeleteValidatedAsync(viewItem);
							}
							finally
							{
								this.OnEndOperation(args);
							}
						};
						dialog.CancelAction = () => this.OnEndOperation(args);
						dialog.DeclineAction = () => this.OnEndOperation(args);
						await dialog.ConfirmAsync(permissionResult.Message);
						break;
					case PermissionStatus.Deny:
						await dialog.DisplayAsync(permissionResult.Message);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			finally
			{
				if (fireEndOperation)
				{
					this.OnEndOperation(args);
				}
			}
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

		private async Task AddValidatedAsync(TViewItem viewItem)
		{
			// Add the item to the db
			var success = await this.Adapter.InsertAsync(viewItem.Item);
			if (success)
			{
				// Find the right index to insert the new element
				var index = this.ViewItems.Count;
				if (this.Sorter.CurrentOption != null)
				{
					index = 0;

					var cmp = this.Sorter.CurrentOption.Comparison;
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
		}

		private async Task UpdateValidatedAsync(TViewItem viewItem)
		{
			// Update the item from the db
			var success = await this.Adapter.UpdateAsync(viewItem.Item);
			if (success)
			{
				this.OnItemUpdated(new ObjectEventArgs<TViewItem>(viewItem));
			}
		}

		private async Task DeleteValidatedAsync(TViewItem viewItem)
		{
			// Delete the item from the db
			var success = await this.Adapter.DeleteAsync(viewItem.Item);
			if (success)
			{
				// Delete the item into the list at the correct place
				this.ViewItems.Remove(viewItem);

				this.OnItemDeleted(new ObjectEventArgs<TViewItem>(viewItem));
			}
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
	}
}