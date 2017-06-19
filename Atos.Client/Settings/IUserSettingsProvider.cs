namespace Atos.Client.Settings
{
	public interface IUserSettingsProvider
	{
		object GetValue(string name);
		void Save(string name, object value);
	}
}