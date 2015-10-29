using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cchbc
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty, string.Empty);

		public string Context { get; }
		public string Name { get; }
		public TimeSpan TimeSpent => this.Stopwatch.Elapsed;
		private Stopwatch Stopwatch { get; }
		public List<FeatureStep> Steps { get; } = new List<FeatureStep>();

		public Feature(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Context = context;
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

		public void AddStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var step = new FeatureStep(name);
			step.TimeSpent = this.Stopwatch.Elapsed;

			// Stop any previous step
			if (this.Steps.Count > 0)
			{
				this.EndStep();
			}

			this.Steps.Add(step);
		}

		public void EndStep()
		{
			var previous = this.Steps[this.Steps.Count - 1];
			previous.TimeSpent = this.Stopwatch.Elapsed - previous.TimeSpent;
		}
	}
}