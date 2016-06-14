using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cchbc.Features
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty, string.Empty);

		public string Context { get; }
		public string Name { get; }
		public TimeSpan TimeSpent => this.Stopwatch.Elapsed;
		public List<FeatureStep> Steps { get; } = new List<FeatureStep>();
		public Action<FeatureStep> StepAdded;
		public Action<FeatureStep> StepEnded;
		public Action<Feature> Started;
		public Action<Feature> Stopped;

		private Stopwatch Stopwatch { get; }

		public static Feature StartNew(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}

		private Feature(string context, string name)
		{
			this.Context = context;
			this.Name = name;
			this.Stopwatch = new Stopwatch();
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

		public FeatureStep AddStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Create new feature step
			var step = new FeatureStep(name, this.Stopwatch.Elapsed);

			// Fire the "event"
			this.StepAdded?.Invoke(step);

			return step;
		}

		public void EndStep(FeatureStep step, string details = null)
		{
			if (step == null) throw new ArgumentNullException(nameof(step));

			step.TimeSpent = this.Stopwatch.Elapsed - step.TimeSpent;
			step.Details = details ?? string.Empty;

			// Add to feature steps
			this.Steps.Add(step);

			this.StepEnded?.Invoke(step);
		}
	}
}