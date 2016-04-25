using System;

namespace Cchbc.Features.Admin.FeatureCountsModule.Objects
{
    public sealed class FeatureCount
    {
        public string Name { get; }
        public string Context { get; }
        public int Value { get; }

        public FeatureCount(string context, string name, int value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (context == null) throw new ArgumentNullException(nameof(context));

            this.Name = name;
            this.Context = context;
            this.Value = value;
        }
    }
}