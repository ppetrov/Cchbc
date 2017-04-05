using System;
using System.Diagnostics;

namespace Cchbc.Features
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty, string.Empty, null);

		private readonly Stopwatch _stopwatch;

		public string Context { get; }
		public string Name { get; }
		public TimeSpan TimeSpent => _stopwatch?.Elapsed ?? TimeSpan.Zero;

		public static Feature StartNew(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new Feature(context, name, Stopwatch.StartNew());
		}

		private Feature(string context, string name, Stopwatch stopwatch)
		{
			this.Context = context;
			this.Name = name;
			_stopwatch = stopwatch;
		}

		public void Pause()
		{
			_stopwatch?.Stop();
		}

		public void Resume()
		{
			_stopwatch?.Start();
		}
	}
}