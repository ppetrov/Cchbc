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
		public TimeSpan TimeSpent => _stopwatch == null ? TimeSpan.Zero : this.Stopwatch.Elapsed;

		public Action<Feature> Started;
		public Action<Feature> Stopped;
		public Action<FeatureStep> StepStarted;
		public Action<FeatureStep> StepEnded;

		private int _level;

		private Stopwatch _stopwatch;
		private Stopwatch Stopwatch => _stopwatch ?? (_stopwatch = new Stopwatch());

		public TimeSpan Elapsed => this.Stopwatch.Elapsed;

		private List<FeatureStep> _steps;
		public List<FeatureStep> Steps => _steps ?? (_steps = new List<FeatureStep>());

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

		public FeatureStep NewStep(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Create new feature step
			var step = new FeatureStep(this, name, ++_level, this.Stopwatch.Elapsed);

			this.StepStarted?.Invoke(step);

			// Add to feature steps
			this.Steps.Add(step);

			return step;
		}

		public void EndStep(FeatureStep step)
		{
			if (step == null) throw new ArgumentNullException(nameof(step));

			var adjustment = TimeSpan.Zero;
			var currentLevel = step.Level;
			for (var i = this.Steps.Count - 1; i >= 0; i--)
			{
				var s = this.Steps[i];
				if (s.Level > currentLevel)
				{
					adjustment += s.TimeSpent;
					continue;
				}
				break;
			}
			if (adjustment != TimeSpan.Zero)
			{
				step.TimeSpent -= adjustment;
			}

			this.StepEnded?.Invoke(step);
			_level--;
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

	public sealed class FeatureData
	{
		public string Context { get; }
		public string Name { get; }
		public TimeSpan TimeSpent { get; }
		public List<FeatureStepData> Steps { get; } = new List<FeatureStepData>();

		public FeatureData(string context, string name, TimeSpan timeSpent)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Context = context;
			this.Name = name;
			this.TimeSpent = timeSpent;
		}
	}
}