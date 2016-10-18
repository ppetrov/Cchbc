using System;
using System.Diagnostics;

namespace Cchbc.Features
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty, string.Empty);

		public string Context { get; }
		public string Name { get; }
		public TimeSpan TimeSpent => _stopwatch == null ? TimeSpan.Zero : this.Stopwatch.Elapsed;

		public Action<Feature> Started;
		public Action<Feature> Stopped;

		private Stopwatch _stopwatch;
		private Stopwatch Stopwatch => _stopwatch ?? (_stopwatch = new Stopwatch());

		public TimeSpan Elapsed => this.Stopwatch.Elapsed;

		public static Feature StartNew(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}

		public Feature(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Context = context;
			this.Name = name;
		}

		public void Start()
		{
			this.Stopwatch.Start();
			this.Started?.Invoke(this);
		}

		public void Stop()
		{
			this.Stopwatch.Stop();
			this.Stopped?.Invoke(this);
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