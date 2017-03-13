using System;

namespace Cchbc.iFSA
{
	public sealed class UserSettingsProvider : IUserSettingsProvider
	{
		public UserSettings Load()
		{
			// TODO : !!! Platform dependant
			return null;
		}

		public void Save(UserSettings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			// TODO : !!! Platform dependant
		}
	}
}