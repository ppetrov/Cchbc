using System;
using System.Diagnostics;

namespace Cchbc
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty);

		public string Name { get; }
		public TimeSpan TimeSpent => this.Stopwatch.Elapsed;
		private Stopwatch Stopwatch { get; }

		public Feature(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.Stopwatch = new Stopwatch();
		}

		public void StartMeasure()
		{
			this.Stopwatch.Start();
		}

		public void StopMeasure()
		{
			this.Stopwatch.Stop();
		}

		public void Pause()
		{
			this.Stopwatch.Stop();
		}

		public void Resume()
		{
			this.Stopwatch.Start();
		}
	}
}