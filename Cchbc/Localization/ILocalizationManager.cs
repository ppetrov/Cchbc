using System.Collections.Generic;

namespace Cchbc.Localization
{
	public interface ILocalizationManager
	{
		void Load(IEnumerable<string> lines);
		string Get(LocalizationKey key);
	}
}