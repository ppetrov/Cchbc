using System;
using Atos.Client;
using ConsoleClient.OrderModule.Models;

namespace ConsoleClient.OrderModule.ViewModels
{
	public sealed class ArticleViewModel : ViewModel
	{
		public Article Article { get; }

		public string Name => this.Article.Name;

		private int _quantity;
		public int Quantity
		{
			get { return _quantity; }
			set { this.SetProperty(ref _quantity, value); }
		}

		public ArticleViewModel(Article article)
		{
			if (article == null) throw new ArgumentNullException(nameof(article));

			this.Article = article;
		}
	}
}