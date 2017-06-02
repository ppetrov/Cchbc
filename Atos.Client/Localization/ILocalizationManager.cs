using System.Collections.Generic;

namespace Atos.Client.Localization
{
	public interface ILocalizationManager
	{
		void Load(IEnumerable<string> lines);
		string Get(LocalizationKey key);
	}
}