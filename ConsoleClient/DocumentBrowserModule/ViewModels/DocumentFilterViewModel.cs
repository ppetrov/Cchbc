using System;
using System.Collections.ObjectModel;
using Atos.Client;

namespace ConsoleClient
{
	public sealed class DocumentFilterViewModel : ViewModel
	{
		private DocumentFilter Filter { get; }

		public string Name => this.Filter.Name;
		public DocumentFilterEntryViewModel[] Entries { get; }
		public ObservableCollection<DocumentFilterEntryViewModel> SelectedEntries { get; } = new ObservableCollection<DocumentFilterEntryViewModel>();

		public DocumentFilterViewModel(DocumentFilter filter, DocumentBrowserViewModel documentBrowserViewModel)
		{
			if (filter == null) throw new ArgumentNullException(nameof(filter));
			if (documentBrowserViewModel == null) throw new ArgumentNullException(nameof(documentBrowserViewModel));

			this.Filter = filter;

			var filterEntries = filter.Entries;
			this.Entries = new DocumentFilterEntryViewModel[filterEntries.Length];
			for (var i = 0; i < filterEntries.Length; i++)
			{
				this.Entries[i] = new DocumentFilterEntryViewModel(filter.Property, filterEntries[i], documentBrowserViewModel);
			}
		}
	}
}