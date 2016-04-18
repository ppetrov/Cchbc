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
		public Action<FeatureStepEventArgs> StepAdded;
		public Action<FeatureEventArgs> Started;
		public Action<FeatureEventArgs> Stopped;

		private Stopwatch Stopwatch { get; }

		public Feature(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Context = context;
			this.Name = name;
			this.Stopwatch = new Stopwatch();
		}

		public void Start()
		{
			this.Resume();

			// Fire the "event"
			this.Started?.Invoke(new FeatureEventArgs(this));
		}

		public void Stop()
		{
			this.Pause();

			// End previous step, if any
			this.EndPreviousStep();

			// Fire the "event"
			this.Stopped?.Invoke(new FeatureEventArgs(this));
		}

		public void Pause()
		{
			// Stop the stopwatch
			this.Stopwatch.Stop();
		}

		public void Resume()
		{
			// Start the stopwatch
			this.Stopwatch.Start();
		}

		public FeatureStep AddStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// End previous step, if any
			this.EndPreviousStep();

			// Create new feature step
			var step = new FeatureStep(name) { TimeSpent = this.Stopwatch.Elapsed };

			// Add to feature steps
			this.Steps.Add(step);

			// Fire the "event"
			this.StepAdded?.Invoke(new FeatureStepEventArgs(step));

			return step;
		}

		private void EndPreviousStep()
		{
			// Has any steps
			if (this.Steps.Count > 0)
			{
				// Take the last one
				var previous = this.Steps[this.Steps.Count - 1];

				// Set the TimeSpent to the delta end - start
				previous.TimeSpent = this.Stopwatch.Elapsed - previous.TimeSpent;
			}
		}
	}
}