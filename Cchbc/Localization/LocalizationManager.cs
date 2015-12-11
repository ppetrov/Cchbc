using System;
using System.Collections.Generic;

namespace Cchbc.Localization
{
	public sealed class LocalizationManager
	{
		private Dictionary<string, string> LocalizationKeys { get; } = new Dictionary<string, string>();

		private Dictionary<string, Tuple<string, string>[]> Values { get; } = new Dictionary<string, Tuple<string, string>[]>();

		// File: but uneditable !!!
		// Can download the latest localization file or switch to another localization : french ???
		// Db : bad we need to sync the entire db
		// TODO : From file
		// need an abstraction probably
		public void Load(IEnumerable<string> lines)
		{
			if (lines == null) throw new ArgumentNullException(nameof(lines));

			this.LocalizationKeys.Clear();

			// Ctx. Name: "Close Day"
			foreach (var line in lines)
			{
				//CalendarScreen.Name:Calendar
				var index = line.IndexOf('.');
				//var name = 
			}

			Values.Add(@"CalendarScreen", new[]
			{
				Tuple.Create(@"Name", @"Calendar"),
				Tuple.Create(@"CloseDay", @"Close Day")
			});

			//CalendarScreen.Name:Calendar
			//CalendarScreen.CloseDay:Close Day
			//CalendarScreen.CancelDay:Cancel Day

			//using (var sr = new StreamReader(null))
			//{
			//	//name:"Name"
			//	//brand:"Brand"
			//	//flavor:"Flavor"
			//	//MsgConfirmDeleteLogin:"Are you sure you want to delete this login?"
			//}
		}

		public string this[LocalizationKey key]
		{
			get
			{
				var result = string.Empty;
				if (key != null)
				{
					var name = key.Name;
					if (!this.LocalizationKeys.TryGetValue(name, out result))
					{
						result = name;
					}
				}
				return result;
			}
		}
	}
}