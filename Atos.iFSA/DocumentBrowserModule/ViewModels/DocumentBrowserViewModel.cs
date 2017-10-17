using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.iFSA.DocumentBrowserModule.Models;

namespace Atos.iFSA.DocumentBrowserModule.ViewModels
{
	public sealed class DocumentBrowserViewModel : ViewModel
	{
		private List<DocumentViewModel> AllDocuments { get; } = new List<DocumentViewModel>();

		public string Title { get; } = @"Documents";

		private string _searchText;
		public string SearchText
		{
			get { return _searchText; }
			set { this.SetProperty(ref _searchText, value); }
		}

		private string _resultsCaption;
		public string ResultsCaption
		{
			get { return _resultsCaption; }
			set { this.SetProperty(ref _resultsCaption, value); }
		}

		private DocumentFilterViewModel _selectedFilter;
		public DocumentFilterViewModel SelectedFilter
		{
			get { return _selectedFilter; }
			set { this.SetProperty(ref _selectedFilter, value); }
		}

		public ObservableCollection<DocumentFilterViewModel> Filters { get; } = new ObservableCollection<DocumentFilterViewModel>();
		public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>();

		public ICommand ClearCommand { get; }
		public ICommand SearchCommand { get; }

		public DocumentBrowserViewModel()
		{
			this.ClearCommand = new ActionCommand(this.Clear);
			this.SearchCommand = new ActionCommand(this.Search);
		}

		public async Task LoadAsync(Func<Task<IEnumerable<DocumentFilter>>> filtersProvider, Func<Task<IEnumerable<Document>>> documentsProvider)
		{
			if (filtersProvider == null) throw new ArgumentNullException(nameof(filtersProvider));
			if (documentsProvider == null) throw new ArgumentNullException(nameof(documentsProvider));

			foreach (var filter in await filtersProvider())
			{
				this.Filters.Add(new DocumentFilterViewModel(filter, this));
			}
			foreach (var document in await documentsProvider())
			{
				this.Documents.Add(new DocumentViewModel(document));
			}

			this.AllDocuments.Clear();
			this.AllDocuments.AddRange(this.Documents);
		}

		public void Add(DocumentFilterEntryViewModel entryViewModel)
		{
			if (entryViewModel == null) throw new ArgumentNullException(nameof(entryViewModel));

			this.AdjustFilterEntries(entryViewModel, (entries, e) => entries.Add(e));
			this.ApplyCurrentFilters();
		}

		public void Remove(DocumentFilterEntryViewModel entryViewModel)
		{
			if (entryViewModel == null) throw new ArgumentNullException(nameof(entryViewModel));

			this.AdjustFilterEntries(entryViewModel, (entries, e) => entries.Remove(e));
			this.ApplyCurrentFilters();
		}

		private void Clear()
		{
			foreach (var filterViewModel in this.Filters)
			{
				filterViewModel.Clear();
			}
			this.ApplyCurrentFilters();
		}

		private void Search()
		{
			this.ApplyCurrentFilters();
		}

		private void ApplyCurrentFilters()
		{
			this.Documents.Clear();

			var search = this.SearchText ?? string.Empty;
			foreach (var document in this.AllDocuments)
			{
				var hasMatch = true;

				if (document.HasMatch(search))
				{
					foreach (var filterViewModel in this.Filters)
					{
						hasMatch &= document.HasMatch(filterViewModel.SelectedEntries);
					}
				}

				if (hasMatch)
				{
					this.Documents.Add(document);
				}
			}

			this.ResultsCaption = $@"{this.Documents.Count} results";
		}

		private void AdjustFilterEntries(DocumentFilterEntryViewModel entryViewModel, Action<ObservableCollection<DocumentFilterEntryViewModel>, DocumentFilterEntryViewModel> operation)
		{
			foreach (var filterViewModel in this.Filters)
			{
				var exists = Exists(filterViewModel, entryViewModel);
				if (exists)
				{
					operation(filterViewModel.SelectedEntries, entryViewModel);
					break;
				}
			}
		}

		private static bool Exists(DocumentFilterViewModel filterViewModel, DocumentFilterEntryViewModel searchEntryViewModel)
		{
			foreach (var entryViewModel in filterViewModel.Entries)
			{
				if (entryViewModel == searchEntryViewModel)
				{
					return true;
				}
			}
			return false;
		}
	}
}