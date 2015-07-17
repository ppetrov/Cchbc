using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cchbc;

namespace Cchbc.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var ctx = new ArticlesContext();
			try
			{
				ctx.Load();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
