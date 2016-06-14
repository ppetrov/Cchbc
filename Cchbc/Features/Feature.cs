using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cchbc.Features
{
    public sealed class Feature
    {
        public static readonly Feature None = new Feature(string.Empty, string.Empty, null, null);

        public string Context { get; }
        public string Name { get; }
        public TimeSpan TimeSpent => this._stopwatch.Elapsed;
        public List<FeatureStep> Steps => _steps ?? (_steps = new List<FeatureStep>());
        public Action<Feature> Started;
        public Action<Feature> Stopped;
        public Action<FeatureStep> StepStarted;
        public Action<FeatureStep> StepEnded;

        private readonly Stopwatch _stopwatch;
        private List<FeatureStep> _steps;

        public static Feature StartNew(string context, string name)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var feature = new Feature(context, name, new Stopwatch(), null);

            feature.Start();

            return feature;
        }

        private Feature(string context, string name, Stopwatch stopwatch, List<FeatureStep> steps)
        {
            this.Context = context;
            this.Name = name;

            _stopwatch = stopwatch;
            _steps = steps;
        }

        public FeatureStep StartStep(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            // Create new feature step
            var step = new FeatureStep(this, name, _stopwatch.Elapsed);

            this.StepStarted?.Invoke(step);

            return step;
        }

        public void EndStep(FeatureStep step, string details = null)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            step.TimeSpent = this._stopwatch.Elapsed - step.TimeSpent;
            step.Details = details ?? string.Empty;

            // Add to feature steps
            this.Steps.Add(step);

            this.StepEnded?.Invoke(step);
        }

        public void Start()
        {
            this._stopwatch.Start();
            this.Started?.Invoke(this);
        }

        public void Stop()
        {
            this._stopwatch.Stop();
            this.Stopped?.Invoke(this);
        }

        public void Pause()
        {
            this._stopwatch.Stop();
        }

        public void Resume()
        {
            this._stopwatch.Start();
        }
    }
}