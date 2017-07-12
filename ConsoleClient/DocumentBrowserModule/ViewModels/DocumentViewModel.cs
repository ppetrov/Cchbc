using System;
using System.Collections.ObjectModel;
using Atos.Client;

namespace ConsoleClient
{
	public sealed class DocumentViewModel : ViewModel
	{
		public Document Document { get; }

		public string Name => this.Document.Name;

		public DocumentViewModel(Document document)
		{
			if (document == null) throw new ArgumentNullException(nameof(document));

			this.Document = document;
		}

		public bool HasMatch(string search)
		{
			if (search == null) throw new ArgumentNullException(nameof(search));

			return this.Document.HasMatch(search);
		}

		public bool HasMatch(ObservableCollection<DocumentFilterEntryViewModel> entries)
		{
			if (entries == null) throw new ArgumentNullException(nameof(entries));

			if (entries.Count > 0)
			{
				var property = entries[0].Property;
				var selectedCodes = new string[entries.Count];
				for (var i = 0; i < selectedCodes.Length; i++)
				{
					selectedCodes[i] = entries[i].Code;
				}
				return this.Document.HasMatch(property, selectedCodes);
			}

			return true;
		}
	}
}