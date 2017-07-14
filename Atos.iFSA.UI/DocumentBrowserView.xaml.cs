using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Atos.iFSA.DocumentBrowserModule.Models;
using Atos.iFSA.DocumentBrowserModule.ViewModels;

namespace Atos.iFSA.UI
{
	public sealed partial class DocumentBrowserView
	{
		public DocumentBrowserViewModel ViewModel { get; } = new DocumentBrowserViewModel();

		public DocumentBrowserView()
		{
			this.InitializeComponent();
			this.DataContext = this.ViewModel;
		}

		private async void DocumentBrowserView_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.ViewModel.LoadAsync(this.GetFilters, this.GetDocuments);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		private Task<IEnumerable<DocumentFilter>> GetFilters()
		{
			var tradeChannelFilter = new DocumentFilter(@"Trade Channel", DocumentProperty.TradeChannel, new[]
			{
				new DocumentFilterEntry(@"01", @"Supermarket"),
				new DocumentFilterEntry(@"02", @"Coffe shop"),
				new DocumentFilterEntry(@"03", @"Restaurant")
			});
			var subTradeChannelFilter = new DocumentFilter(@"Sub Trade Channel", DocumentProperty.SubTradeChannel, new[]
			{
				new DocumentFilterEntry(@"01", @"Big Supermarket"),
				new DocumentFilterEntry(@"02", @"Small Supermarket"),
				new DocumentFilterEntry(@"03", @"Medium Supermarket")
			});
			IEnumerable<DocumentFilter> filters = new[]
			{
				tradeChannelFilter,
				subTradeChannelFilter
			};
			return Task.FromResult(filters);
		}

		private Task<IEnumerable<Document>> GetDocuments()
		{
			IEnumerable<Document> documents = new[]
			{
				new Document(1, @"Coca Cola"),
				new Document(2, @"Fanta"),
				new Document(3, @"Sprite"),
			};

			return Task.FromResult(documents);
		}


	}
}
