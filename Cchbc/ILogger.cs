namespace Cchbc
{
	public interface ILogger
	{
		string Context { get; }

		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsWarnEnabled { get; }
		bool IsErrorEnabled { get; }

		void Debug(string message);
		void Info(string message);
		void Warn(string message);
		void Error(string message);
	}
}