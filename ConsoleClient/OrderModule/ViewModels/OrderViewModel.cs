using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Atos.Client;
using Atos.Client.Common;
using ConsoleClient.OrderModule.Data;
using ConsoleClient.OrderModule.Models;

namespace ConsoleClient.OrderModule.ViewModels
{
	public sealed class OrderViewModel : ViewModel
	{
		private Order Order { get; set; }
		private List<ArticleViewModel> AllArticles { get; } = new List<ArticleViewModel>();

		public OrderHeaderViewModel Header { get; private set; }
		public ObservableCollection<ArticleViewModel> Articles { get; } = new ObservableCollection<ArticleViewModel>();

		public ICommand SaveCommand { get; }

		public OrderViewModel()
		{
			this.SaveCommand = new ActionCommand(this.Save);
		}

		private string _searchText;
		public string SearchText
		{
			get { return _searchText; }
			set
			{
				this.SetProperty(ref _searchText, value);
				this.ApplyCurrentSearch();
			}
		}

		public Task InitializeAsync()
		{
			var dataProvider = default(IOrderDataProvider);

			var orderHeader = new OrderHeader();
			this.Order = new Order(orderHeader);
			this.Header = new OrderHeaderViewModel(orderHeader);

			this.Articles.Clear();
			foreach (var article in dataProvider.GetArticles())
			{
				this.Articles.Add(new ArticleViewModel(article));
			}

			this.AllArticles.Clear();
			this.AllArticles.AddRange(this.Articles);

			return Task.CompletedTask;
		}

		private void Save()
		{
			var dataProvider = default(IOrderDataProvider);

			var orderDetails = this.Order.Details;

			orderDetails.Clear();
			foreach (var viewModel in this.AllArticles)
			{
				if (viewModel.Quantity != 0)
				{
					orderDetails.Add(new OrderDetail(viewModel.Article, viewModel.Quantity));
				}
			}

			dataProvider.Save(this.Order);
		}

		private void ApplyCurrentSearch()
		{
			this.Articles.Clear();
			foreach (var viewModel in this.AllArticles)
			{
				if (viewModel.Name.IndexOf(this.SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					this.Articles.Add(viewModel);
				}
			}
		}
	}
}