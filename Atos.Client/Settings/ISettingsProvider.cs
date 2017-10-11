namespace Atos.Client.Settings
{
	public interface ISettingsProvider
	{
		object GetValue(string name);
		void Save(string name, object value);
	}
}