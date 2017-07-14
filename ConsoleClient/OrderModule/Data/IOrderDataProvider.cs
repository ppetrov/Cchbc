using System.Collections.Generic;
using ConsoleClient.OrderModule.Models;

namespace ConsoleClient.OrderModule.Data
{
	public interface IOrderDataProvider
	{
		List<Article> GetArticles();
		void Save(Order order);
	}
}