using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc
{
	public class Manager<T> where T : IModifiableObject
	{
		private List<T> ViewItems { get; set; }

		public event EventHandler<ObjectEventArgs<T>> ItemInserted;
		protected virtual void OnItemInserted(ObjectEventArgs<T> e)
		{
			this.ItemInserted?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<T>> ItemUpdated;
		protected virtual void OnItemUpdated(ObjectEventArgs<T> e)
		{
			this.ItemUpdated?.Invoke(this, e);
		}

		public event EventHandler<ObjectEventArgs<T>> ItemDeleted;
		protected virtual void OnItemDeleted(ObjectEventArgs<T> e)
		{
			this.ItemDeleted?.Invoke(this, e);
		}

		public IModifiableAdapter<T> Adapter { get; }
		public Sorter<T> Sorter { get; }
		public Searcher<T> Searcher { get; }
		public FilterOption<T>[] FilterOptions { get; set; }

		public Manager(IModifiableAdapter<T> adapter, Sorter<T> sorter, Searcher<T> searcher, FilterOption<T>[] filterOptions = null)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Adapter = adapter;
			this.Sorter = sorter;
			this.Searcher = searcher;
			this.FilterOptions = filterOptions;
		}

		public void LoadData(List<T> viewItems)
		{
			if (viewItems == null) throw new ArgumentNullException(nameof(viewItems));

			this.ViewItems = viewItems;
			this.Sorter.Sort(this.ViewItems, this.Sorter.CurrentOption);
		}

		public async Task AddAsync(T viewItem, ModalDialog dialog)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));

			var validationResults = this.ValidateProperties(viewItem);
			if (validationResults.Length == 0)
			{
				// Apply business logic
				var permissionResult = this.CanAdd(viewItem);
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
						//await this.Dialog.DisplayAsync(permissionResult.Message);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private async Task AddValidatedAsync(T viewItem)
		{
			// Call the manager to add the new message
			if (await this.AddAsync(viewItem, true))
			{
				// Add the item to the list to the right place if sorter != null
				this.ViewItems.Add(viewItem);
			}
			throw new Exception(@"PPPetrov");
		}

		private async Task<bool> AddAsync(T viewItem, bool confirmed)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var validationResults = this.ValidateProperties(viewItem);
			if (validationResults.Length == 0)
			{
				var permissionResult = this.CanAdd(viewItem);
				if (permissionResult.Status == PermissionStatus.Allow ||
					(permissionResult.Status == PermissionStatus.Confirm && confirmed))
				{
					// Add the item to the db
					await this.Adapter.InsertAsync(viewItem);

					// Add the item to the list
					this.ViewItems.Add(viewItem);

					// TODO : !!!! Fire event to attach for additional logic

					return true;
				}
			}

			return false;
		}

		// TODO : !!! public ???
		// TODO : !!! abstract ???
		private PermissionResult CanAdd(T viewItem)
		{
			return PermissionResult.Allow;
        }

		// TODO : !!! public ???
		// TODO : !!! abstract ???
		private ValidationResult[] ValidateProperties(T viewItem)
		{
			return Enumerable.Empty<ValidationResult>().ToArray();
		}

		public IEnumerable<T> Sort(ICollection<T> currentViewItems, SortOption<T> sortOption)
		{
			if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

			var flag = sortOption.Ascending ?? true;

			// Sort view items
			this.Sorter.Sort(this.ViewItems, sortOption);

			// Sort current view items
			var copy = new T[currentViewItems.Count];
			currentViewItems.CopyTo(copy, 0);
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

		public IEnumerable<T> Search(string textSearch, SearchOption<T> searchOption)
		{
			if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

			return this.Searcher.Search(GetInputViewItems(this.ViewItems), textSearch, searchOption);
		}

		private ICollection<T> GetInputViewItems(ICollection<T> viewItems)
		{
			if (this.FilterOptions != null && this.FilterOptions.Length > 0)
			{
				viewItems = new List<T>();

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