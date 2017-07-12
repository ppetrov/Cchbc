using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;

namespace ConsoleClient
{
	public sealed class DocumentFilterEntryViewModel : ViewModel
	{
		private DocumentFilterEntry Entry { get; }
		private DocumentBrowserViewModel DocumentBrowserViewModel { get; }

		public DocumentProperty Property { get; }
		public string Name => this.Entry.Name;
		public string Code => this.Entry.Code;

		public ICommand AddCommand { get; }
		public ICommand RemoveCommand { get; }

		public DocumentFilterEntryViewModel(DocumentProperty property, DocumentFilterEntry entry, DocumentBrowserViewModel documentBrowserViewModel)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (documentBrowserViewModel == null) throw new ArgumentNullException(nameof(documentBrowserViewModel));

			this.Property = property;
			this.Entry = entry;
			this.DocumentBrowserViewModel = documentBrowserViewModel;
			this.AddCommand = new RelayCommand(() =>
			{
				this.DocumentBrowserViewModel.Add(this);
			});
			this.RemoveCommand = new RelayCommand(() =>
			{
				this.DocumentBrowserViewModel.Remove(this);
			});
		}
	}
}