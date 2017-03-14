namespace iFSA
{
	public interface IUserSettingsProvider
	{
		UserSettings Load();
		void Save(UserSettings settings);
	}
}