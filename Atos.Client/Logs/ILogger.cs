namespace Atos.Client.Logs
{
	public interface ILogger
	{
		void Log(string message, LogLevel level);
	}
}