namespace Cchbc.Logs
{
	public interface ILog
	{
		void Log(string message, LogLevel level);
	}
}