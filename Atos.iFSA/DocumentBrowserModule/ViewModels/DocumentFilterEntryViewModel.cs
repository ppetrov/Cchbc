using System;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using Atos.iFSA.DocumentBrowserModule.Models;

namespace Atos.iFSA.DocumentBrowserModule.ViewModels
{
	public sealed class DocumentFilterEntryViewModel : ViewModel
	{
		private DocumentFilterEntry Entry { get; }
		private DocumentBrowserViewModel DocumentBrowserViewModel { get; }

		public DocumentProperty Property { get; }
		public string Name => this.Entry.Name;
		public string Code => this.Entry.Code;

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				this.SetProperty(ref _isSelected, value);
				this.CanAdd = !this.IsSelected;
			}
		}

		private bool _canAdd = true;
		public bool CanAdd
		{
			get { return _canAdd; }
			private set { this.SetProperty(ref _canAdd, value); }
		}

		public ICommand AddCommand { get; }
		public ICommand RemoveCommand { get; }

		public DocumentFilterEntryViewModel(DocumentProperty property, DocumentFilterEntry entry, DocumentBrowserViewModel documentBrowserViewModel)
		{
			if (entry == null) throw new ArgumentNullException(nameof(entry));
			if (documentBrowserViewModel == null) throw new ArgumentNullException(nameof(documentBrowserViewModel));

			this.Property = property;
			this.Entry = entry;
			this.DocumentBrowserViewModel = documentBrowserViewModel;
			this.AddCommand = new ActionCommand(() =>
			{
				this.IsSelected = true;
				this.DocumentBrowserViewModel.Add(this);
			});
			this.RemoveCommand = new ActionCommand(() =>
			{
				this.IsSelected = false;
				this.DocumentBrowserViewModel.Remove(this);
			});
		}
	}
}