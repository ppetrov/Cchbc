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

        private int _stepLevel = 1;

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

        public FeatureStep StartNestedStep(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return StartNewStep(name, true);
        }

        public FeatureStep StartStep(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return StartNewStep(name, false);
        }

        private FeatureStep StartNewStep(string name, bool isParent)
        {
            // Create new feature step
            var step = new FeatureStep(this, name, _stepLevel, isParent, this.Stopwatch.Elapsed);

            this.StepStarted?.Invoke(step);

            // Add to feature steps
            this.Steps.Add(step);

            if (step.IsParent)
            {
                _stepLevel++;
            }

            return step;
        }

        public void EndStep(FeatureStep step, string details = null)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            step.TimeSpent = this.Stopwatch.Elapsed - step.TimeSpent;
            step.Details = details ?? string.Empty;

            this.StepEnded?.Invoke(step);

            if (step.IsParent)
            {
                _stepLevel--;
            }
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