using System;

namespace iFSA
{
	public static class UserSettingsProvider
	{
		public static UserSettings Load()
		{
			// TODO : !!! Platform dependant
			return null;
		}

		public static void Save(UserSettings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			// TODO : !!! Platform dependant
		}
	}
}