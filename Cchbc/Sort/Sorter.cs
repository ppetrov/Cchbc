using System;
using System.Collections.ObjectModel;
using Cchbc.Objects;

namespace Cchbc.Sort
{
    public class Sorter<T> : ViewObject where T : ViewObject
    {
        public SortOption<T>[] Options { get; private set; }

        private SortOption<T> _currentOption;
        public SortOption<T> CurrentOption
        {
            get { return _currentOption; }
            private set { this.SetField(ref _currentOption, value); }
        }

        public SortOption<T> DefaultOption { get; }

        private bool? _ascending;
        public bool? Ascending
        {
            get { return _ascending; }
            private set { this.SetField(ref _ascending, value); }
        }

        public Sorter(SortOption<T>[] options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Length == 0) throw new ArgumentOutOfRangeException(nameof(options));

            this.Options = options;
            foreach (var option in options)
            {
                if (option.IsDefault)
                {
                    this.DefaultOption = option;
                    break;
                }
            }
        }

        public void Sort(ObservableCollection<T> items, SortOption<T> option, bool? descending = null)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (option == null) throw new ArgumentNullException(nameof(option));

            var ascending = (this.CurrentOption != option);
            if (descending.HasValue && descending.Value)
            {
                ascending = false;
            }

            // Set the current option
            this.CurrentOption = option;
            this.Ascending = ascending;

            // Create a copy of the items
            var copy = new T[items.Count];
            items.CopyTo(copy, 0);

            if (ascending)
            {
                // Sort the copy in ascending order
                Array.Sort(copy, new Comparison<T>(option.Comparison));
            }
            else
            {
                if (descending ?? false)
                {
                    // Sort the copy in descending order
                    var cmp = new Comparison<T>((x, y) => option.Comparison(y, x));
                    Array.Sort(copy, cmp);
                }
                else
                {
                    // The array is already sorted, just reverse it.
                    Array.Reverse(copy);
                }
            }

            // Overwrite the items with the sorted ones
            for (var i = 0; i < copy.Length; i++)
            {
                items[i] = copy[i];
            }
        }
    }
}