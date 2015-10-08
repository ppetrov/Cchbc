using System;
using System.Collections.Generic;
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

		public async Task AddAsync(TViewItem viewItem, ModalDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));

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
						// Confirm any user warnings
						dialog.AcceptAction = () => this.AddValidatedAsync(viewItem);
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

		public async Task DeleteAsync(TViewItem viewItem, ModalDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));

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
					var cmp = this.Sorter.CurrentOption.Comparison;
					foreach (var current in this.ViewItems)
					{
						var result = cmp(current, viewItem);
						if (result >= 0)
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

		public IEnumerable<TViewItem> Search(string textSearch, SearchOption<TViewItem> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			return this.Searcher.Search(GetFilteredViewItems(this.ViewItems), textSearch, searchOption);
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