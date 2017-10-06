using System.Diagnostics;
using Atos.Client.Logs;

namespace Atos.iFSA.UI
{
	public sealed class Logger : ILogger
	{
		public void Log(string message, LogLevel level)
		{
			Debug.WriteLine(level + ":" + message);
		}
	}
}