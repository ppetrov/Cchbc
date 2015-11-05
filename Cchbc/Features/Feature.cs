using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cchbc.Features
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty, string.Empty, string.Empty);

		public string Context { get; }
		public string Name { get; }
		public string Details { get; }
		public TimeSpan TimeSpent => this.Stopwatch.Elapsed;
		public List<FeatureStep> Steps { get; } = new List<FeatureStep>();

		private Stopwatch Stopwatch { get; }

		public Feature(string context, string name, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Context = context;
			this.Name = name;
			this.Details = details;
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

		public FeatureStep AddStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var step = new FeatureStep(name);
			step.TimeSpent = this.Stopwatch.Elapsed;

			this.EndStep();
			this.Steps.Add(step);

			return step;
		}

		public void EndStep()
		{
			if (this.Steps.Count > 0)
			{
				var previous = this.Steps[this.Steps.Count - 1];
				previous.TimeSpent = this.Stopwatch.Elapsed - previous.TimeSpent;
			}
		}
	}
}