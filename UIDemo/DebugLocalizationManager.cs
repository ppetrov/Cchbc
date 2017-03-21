using System.Collections.Generic;
using System.Diagnostics;
using Cchbc.Localization;

namespace UIDemo
{
	public sealed class DebugLocalizationManager : ILocalizationManager
	{
		public void Load(IEnumerable<string> lines)
		{
			Debug.WriteLine(@"Load localization manager");
		}

		public string Get(LocalizationKey key)
		{
			Debug.WriteLine(@"Get localization key");
			return @"N/A";
		}
	}
}