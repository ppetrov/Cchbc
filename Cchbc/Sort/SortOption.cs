using System;
using Cchbc.Objects;

namespace Cchbc.Sort
{
    public class SortOption<T> where T : ViewObject
    {
        public string DisplayName { get; private set; }
        public Func<T, T, int> Comparison { get; private set; }
        public bool IsDefault { get; private set; }

        public SortOption(string displayName, Func<T, T, int> comparison, bool isDefault = false)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));

            this.DisplayName = displayName;
            this.Comparison = comparison;
            this.IsDefault = isDefault;
        }
    }
}