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

		public static Feature StartNew(string context, string name, string details = null)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var feature = new Feature(context, name, details ?? string.Empty);

			feature.StartMeasure();

			return feature;
		}

		public void StartMeasure()
		{
			this.Stopwatch.Start();
		}

		public void StopMeasure()
		{
			this.Stopwatch.Stop();
		}

		public void FinishMeasure()
		{
			this.EndPreviousStep();
			this.StopMeasure();
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

			this.EndPreviousStep();

			var step = new FeatureStep(name) { TimeSpent = this.Stopwatch.Elapsed };

			this.Steps.Add(step);

			return step;
		}

		private void EndPreviousStep()
		{
			if (this.Steps.Count > 0)
			{
				var previous = this.Steps[this.Steps.Count - 1];
				previous.TimeSpent = this.Stopwatch.Elapsed - previous.TimeSpent;
			}
		}
	}
}