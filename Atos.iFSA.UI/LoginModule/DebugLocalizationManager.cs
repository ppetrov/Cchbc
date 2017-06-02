using System;
using System.Collections.Generic;
using Atos.Client.Localization;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed class DebugLocalizationManager : ILocalizationManager
	{
		public void Load(IEnumerable<string> lines)
		{
			throw new System.NotImplementedException();
		}

		public string Get(LocalizationKey key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			return @"--->" + key.Name;
		}
	}
}