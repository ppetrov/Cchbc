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

        private Stopwatch _stopwatch;
        private Stopwatch Stopwatch => _stopwatch ?? (_stopwatch = new Stopwatch());

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

        public FeatureStep StartStep(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            // Create new feature step
            var step = new FeatureStep(this, name, this.Stopwatch.Elapsed);

            this.StepStarted?.Invoke(step);

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