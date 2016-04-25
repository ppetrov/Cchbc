using System;

namespace Cchbc.Features.Admin.FeatureCountsModule.Objects
{
    public sealed class ExceptionCount
    {
        public string Context { get; }
        public string Feature { get; }
        public int Value { get; }

        public ExceptionCount(string context, string feature, int value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (feature == null) throw new ArgumentNullException(nameof(feature));

            this.Context = context;
            this.Feature = feature;
            this.Value = value;
        }
    }
}