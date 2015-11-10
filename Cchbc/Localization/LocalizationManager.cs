using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cchbc.Localization
{
	public sealed class LocalizationManager
	{
		private Dictionary<string, string> LocalizationKeys { get; } = new Dictionary<string, string>();

		// File: but uneditable !!!
		// Can download the latest localization file or switch to another localization : french ???
		// Db : bad we need to sync the entire db
		// TODO : From file
		// need an abstraction probably
		public Task LoadAsync()
		{
			this.LocalizationKeys.Clear();

			//using (var sr = new StreamReader(null))
			//{
			//	//name:"Name"
			//	//brand:"Brand"
			//	//flavor:"Flavor"
			//	//MsgConfirmDeleteLogin:"Are you sure you want to delete this login?"
			//}

			return null;
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