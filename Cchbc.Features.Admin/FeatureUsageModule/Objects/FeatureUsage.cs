using System;

namespace Cchbc.Features.Admin.FeatureUsageModule.Objects
{
    public sealed class FeatureUsage
    {
        public string Name { get; }
        public string Context { get; }
        public int Count { get; }

        public FeatureUsage(string context, string name, int count)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (context == null) throw new ArgumentNullException(nameof(context));

            this.Name = name;
            this.Context = context;
            this.Count = count;
        }
    }
}