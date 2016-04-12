using System;

namespace Cchbc.ConsoleClient
{
	public sealed class Person
	{
		public string Name { get; }
		public DateTime BDate { get; }

		public Person(string name, DateTime bDate)
		{
			Name = name;
			BDate = bDate;
		}
	}
}